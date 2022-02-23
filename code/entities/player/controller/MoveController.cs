using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class MoveController : BasePlayerController
	{
		[Net, Predicted] public Vector3 Impulse { get; set; }
		[Net, Predicted] public bool IsJetpacking { get; set; }
		[Net, Predicted] public bool IsSkiing { get; set; }
		[Net] public float JetpackScale { get; set; }
		[Net] public float MaxSpeed { get; set; }
		[Net] public float MoveSpeed { get; set; }
		public TimeSince LastSkiTime { get; set; }

		public bool OnlyRegenJetpackOnGround { get; set; } = true;
		public float PostSkiFrictionTime { get; set; } = 1.5f;
		public float SkiStrafeControl { get; set; } = 0.5f;
		public float JetpackSurfaceBounce { get; set; } = 0.3f;
		public float SkiWallBounce { get; set; } = 0.5f;
		public float FallDamageThreshold { get; set; } = 600f;
		public float GroundSlideScale { get; set; } = 0.85f;
		public float SlideUphillScale { get; set; } = 1.5f;
		public float SlideDownhillScale { get; set; } = 1.1f;
		public float MinUpSlopeAngle { get; set; } = 100f;
		public float MoveSpeedScale { get; set; } = 1f;
		public float MaxJetpackVelocity { get; set; } = 400f;
		public float JetpackAimThrust { get; set; } = 20f;
		public float JetpackBoost { get; set; } = 700f;
		public float Acceleration { get; set; } = 10f;
		public float AirAcceleration { get; set; } = 50f;
		public float GroundFriction { get; set; } = 4f;
		public float StopSpeed { get; set; } = 100f;
		public float FallDamageMin { get; set; } = 0f;
		public float FallDamageMax { get; set; } = 400f;
		public float StayOnGroundAngle { get; set; } = 270.0f;
		public float GroundAngle { get; set; } = 46.0f;
		public float StepSize { get; set; } = 18.0f;
		public float MaxNonJumpVelocity { get; set; } = 140f;
		public float BodyGirth { get; set; } = 32f;
		public float BodyHeight { get; set; } = 72f;
		public float EyeHeight { get; set; } = 64f;
		public float Gravity { get; set; } = 800f;
		public float AirControl { get; set; } = 30f;
		public bool Swimming { get; set; } = false;

		protected Unstuck Unstuck { get; private set; }

		protected float SurfaceFriction { get; set; }
		protected Vector3 PreVelocity { get; set; }
		protected Vector3 Mins { get; set; }
		protected Vector3 Maxs { get; set; }
		protected bool IsTouchingLadder { get; set; }
		protected Vector3 LadderNormal { get; set; }
		protected Player Player { get; set; }

		public MoveController()
		{
			Unstuck = new Unstuck( this );
		}

		public void ClearGroundEntity()
		{
			if ( GroundEntity == null ) return;

			GroundEntity = null;
			GroundNormal = Vector3.Up;
			SurfaceFriction = 1.0f;
		}

		public override BBox GetHull()
		{
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, BodyHeight );
			return new BBox( mins, maxs );
		}

		private void SetBBox( Vector3 mins, Vector3 maxs )
		{
			if ( Mins == mins && Maxs == maxs )
				return;

			Mins = mins;
			Maxs = maxs;
		}

		private void UpdateBBox()
		{
			var girth = BodyGirth * 0.5f;
			var mins = Scale( new Vector3( -girth, -girth, 0 ) );
			var maxs = Scale( new Vector3( +girth, +girth, BodyHeight ) );

			SetBBox( mins, maxs );
		}

		public override void Simulate()
		{
			if ( Pawn is not Player player ) return;

			EyeLocalPosition = Vector3.Up * Scale( EyeHeight );
			UpdateBBox();

			EyeLocalPosition += TraceOffset;
			EyeRotation = Input.Rotation;
			Player = player;

			if ( Unstuck.TestAndFix() )
			{
				// I hope this never really happens.
				return;
			}

			var tr = TraceBBox( Position, Position + Vector3.Down * 8f, 16f );

			if ( tr.Hit )
			{
				UpdateGroundEntity( tr );
			}

			if ( Impulse.Length > 0 )
			{
				ClearGroundEntity();
				Velocity += Impulse;
				Impulse = 0f;
			}

			CheckLadder();
			Swimming = Pawn.WaterLevel > 0.6f;

			PreVelocity = Velocity;

			if ( !Swimming && !IsTouchingLadder && !IsJetpacking )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;
				BaseVelocity = BaseVelocity.WithZ( 0 );
			}

			IsJetpacking = false;
			IsSkiing = false;

			if ( Input.Down( Input.UsingController ? InputButton.Jump : InputButton.Attack2 ) )
			{
				DoJetpackMovement();
			}

			var startOnGround = GroundEntity != null;

			if ( startOnGround )
			{
				Velocity = Velocity.WithZ( 0 );

				if ( Input.Down( Input.UsingController ? InputButton.Attack2 : InputButton.Jump ) )
				{
					HandleSki();
				}
				else
				{
					var skiFriction = MathF.Min( (LastSkiTime / PostSkiFrictionTime), 1f );
					ApplyFriction( GroundFriction * SurfaceFriction * skiFriction );
				}
			}

			if ( Host.IsServer )
			{
				if ( !IsJetpacking && (startOnGround || !OnlyRegenJetpackOnGround) )
				{
					Player.IsRegeneratingEnergy = true;
					Player.Energy = (Player.Energy + Player.EnergyRegen * Time.Delta).Clamp( 0f, Player.MaxEnergy );
				}
				else
				{
					Player.IsRegeneratingEnergy = false;
				}
			}

			WishVelocity = new Vector3( Input.Forward, Input.Left, 0 );
			var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
			WishVelocity *= Input.Rotation;

			if ( !Swimming && !IsTouchingLadder && !IsJetpacking )
			{
				WishVelocity = WishVelocity.WithZ( 0 );
			}

			WishVelocity = WishVelocity.Normal * inSpeed;
			WishVelocity *= GetWishSpeed();

			var stayOnGround = false;

			if ( Swimming )
			{
				ApplyFriction( 1f );
				WaterMove();
			}
			else if ( IsTouchingLadder )
			{
				LadderMove();
			}
			else if ( GroundEntity != null )
			{
				stayOnGround = true;
				WalkMove();
			}
			else
			{
				Velocity -= Velocity * 0.05f * Time.Delta;
				AirMove();
			}

			if ( !IsJetpacking )
				CategorizePosition( stayOnGround );
			else
				ClearGroundEntity();

			if ( !Swimming && !IsTouchingLadder && !IsJetpacking )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			}

			if ( GroundEntity != null )
			{
				Velocity = Velocity.WithZ( 0 );
			}

			if ( IsSkiing ) SetTag( "skiing" );
		}

		private float GetWishSpeed()
		{
			var speed = Scale( MoveSpeed * MoveSpeedScale );

			if ( IsSkiing )
				return speed * SkiStrafeControl;
			else
				return speed;
		}

		private void WalkMove()
		{
			var wishDir = WishVelocity.Normal;
			var wishSpeed = WishVelocity.Length;

			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.Normal * wishSpeed;

			Velocity = Velocity.WithZ( 0 );

			Accelerate( wishDir, wishSpeed, 0f, Acceleration );

			if ( IsSkiing && !IsJetpacking )
			{
				var trace = Trace.Ray( Position, Position + Vector3.Down * 100f )
					.Ignore( Pawn )
					.Run();

				var direction = Velocity.WithZ( 0f ).Normal;
				var angle = trace.Normal.Angle( direction );
				var scale = GroundSlideScale;

				if ( angle > 90f )
				{
					// We're going uphill.
					scale = angle.Remap( 90f, 120f, scale, SlideUphillScale );
				}
				else
				{
					// We're going downhill.
					scale = angle.Remap( 60f, 90f, SlideDownhillScale, scale );
				}

				Velocity += trace.Normal.WithZ( 0f ) * Velocity.Length * scale * Time.Delta;
				Velocity = Velocity.ClampLength( MaxSpeed );
			}

			Velocity = Velocity.WithZ( 0 );
			Velocity += BaseVelocity;

			try
			{
				if ( Velocity.Length < 1.0f )
				{
					Velocity = Vector3.Zero;
					return;
				}

				var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );
				var pm = TraceBBox( Position, dest );

				if ( pm.Fraction == 1 )
				{
					Position = pm.EndPosition;
					StayOnGround();
					return;
				}

				StepMove();
			}
			finally
			{
				Velocity -= BaseVelocity;
			}

			StayOnGround();
		}

		private void StepMove()
		{
			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
			mover.WallBounce = IsSkiing ? SkiWallBounce : 0f;
			mover.MaxStandableAngle = GroundAngle;
			mover.TryMoveWithStep( Time.Delta, StepSize );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		private void Move()
		{
			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
			mover.WallBounce = IsJetpacking ? JetpackSurfaceBounce : 0f;
			mover.MaxStandableAngle = GroundAngle;
			mover.TryMove( Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		private void Accelerate( Vector3 wishDir, float wishSpeed, float speedLimit, float acceleration )
		{
			if ( speedLimit > 0 && wishSpeed > speedLimit )
				wishSpeed = speedLimit;

			var currentSpeed = Velocity.Dot( wishDir );
			var addSpeed = wishSpeed - currentSpeed;

			if ( addSpeed <= 0 )
				return;

			var accelSpeed = acceleration * Time.Delta * wishSpeed * SurfaceFriction;

			if ( accelSpeed > addSpeed )
				accelSpeed = addSpeed;

			if ( IsSkiing || LastSkiTime < 0.5f )
			{
				var previousLength = Math.Max( Velocity.Length, wishSpeed );
				Velocity += wishDir * accelSpeed;
				Velocity = Velocity.ClampLength( previousLength );
			}
			else
			{
				Velocity += wishDir * accelSpeed;
			}
		}

		private void ClassicAccelerate( Vector3 wishDir, float wishSpeed, float speedLimit )
		{
			if ( speedLimit > 0 && wishSpeed > speedLimit )
				wishSpeed = speedLimit;

			var wishVelocity = wishDir * wishSpeed;
			var pushDir = wishVelocity - Velocity;
			var pushLen = pushDir.Length;
			var canPush = 1f * Time.Delta * wishSpeed;
			
			if ( canPush > pushLen )
				canPush = pushLen;
			
			Velocity += (pushDir * canPush * Time.Delta);
		}

		private void HandleSki()
		{
			LastSkiTime = 0f;
			IsSkiing = true;
		}

		private void ApplyFriction( float frictionAmount = 1.0f )
		{
			var speed = Velocity.Length;
			if ( speed < 0.1f ) return;

			var control = (speed < StopSpeed) ? StopSpeed : speed;
			var drop = control * Time.Delta * frictionAmount;
			var newSpeed = speed - drop;

			if ( newSpeed < 0 ) newSpeed = 0;

			if ( newSpeed != speed )
			{
				newSpeed /= speed;
				Velocity *= newSpeed;
			}
		}

		private void DoJetpackMovement()
		{
			if ( Swimming )
			{
				ClearGroundEntity();
				Velocity = Velocity.WithZ( 100 );
				return;
			}

			var startZ = Velocity.z;

			if ( GroundEntity == null )
			{
				if ( Player.Energy >= 5f )
				{
					IsJetpacking = true;

					Velocity += Velocity.WithZ( 0f ).Normal * Scale( JetpackAimThrust * JetpackScale ) * Time.Delta;

					if ( Velocity.z <= MaxJetpackVelocity )
					{
						Velocity += Vector3.Up * Gravity * Time.Delta;

						var boost = Scale( JetpackBoost ) * JetpackScale * 2f;
						var trace = Trace.Ray( Position, Vector3.Up * boost * Time.Delta * 8f )
							.Ignore( Pawn )
							.Run();

						if ( !trace.Hit )
                        {
							Velocity += Vector3.Up * boost * Time.Delta;
						}
					}
				}

				if ( Host.IsServer && !Player.InEnergyElevator )
				{
					Player.Energy = (Player.Energy - Player.EnergyDrain * Time.Delta).Clamp( 0f, Player.MaxEnergy );
				}
			}
			else if ( Player.Energy >= 5 )
			{
				ClearGroundEntity();

				var groundFactor = 0.5f;
				var multiplier = Scale( 268.3281572999747f * 1.2f );

				Velocity = Velocity.WithZ( startZ + multiplier * groundFactor );
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

				AddEvent( "jump" );
			}
		}

		private float Scale( float speed )
		{
			return speed * Pawn.Scale;
		}

		private Vector3 Scale( Vector3 velocity )
		{
			return velocity * Pawn.Scale;
		}

		private void AirMove()
		{
			var wishDir = WishVelocity.Normal;
			var wishSpeed = WishVelocity.Length;

			Accelerate( wishDir, wishSpeed, AirControl, AirAcceleration );

			Velocity += BaseVelocity;

			Move();

			Velocity -= BaseVelocity;
		}

		private void WaterMove()
		{
			var wishDir = WishVelocity.Normal;
			var wishSpeed = WishVelocity.Length;

			wishSpeed *= 0.8f;

			Accelerate( wishDir, wishSpeed, 100f, Acceleration );

			Velocity += BaseVelocity;

			Move();

			Velocity -= BaseVelocity;
		}

		private void CheckLadder()
		{
			if ( IsTouchingLadder && Input.Pressed( InputButton.Jump ) )
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;
				return;
			}

			var ladderDistance = 1.0f;
			var start = Position;
			var end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : WishVelocity.Normal) * ladderDistance;

			var pm = Trace.Ray( start, end )
				.Size( Mins, Maxs )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.LADDER, true )
				.Ignore( Pawn )
				.Run();

			IsTouchingLadder = false;

			if ( pm.Hit )
			{
				IsTouchingLadder = true;
				LadderNormal = pm.Normal;
			}
		}

		private void LadderMove()
		{
			var velocity = WishVelocity;
			var normalDot = velocity.Dot( LadderNormal );
			var cross = LadderNormal * normalDot;

			Velocity = (velocity - cross) + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

			Move();
		}

		private void CategorizePosition( bool stayOnGround )
		{
			SurfaceFriction = 1.0f;

			var point = Position - Vector3.Up * 2f;
			var bumpOrigin = Position;
			var moveToEndPos = false;

			if ( GroundEntity != null )
			{
				moveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( stayOnGround )
			{
				moveToEndPos = true;
				point.z -= StepSize;
			}

			if ( Velocity.z > MaxNonJumpVelocity || Swimming )
			{
				ClearGroundEntity();
				return;
			}

			var pm = TraceBBox( bumpOrigin, point, 16f );

			if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > StayOnGroundAngle )
			{
				ClearGroundEntity();
				moveToEndPos = false;

				if ( Velocity.z > 0 )
					SurfaceFriction = 0.25f;
			}
			else
			{
				UpdateGroundEntity( pm );
			}

			if ( moveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				Position = pm.EndPosition;
			}
		}

		private void UpdateGroundEntity( TraceResult trace )
		{
			var wasOnGround = (GroundEntity != null);

			GroundNormal = trace.Normal;
			GroundEntity = trace.Entity;
			SurfaceFriction = trace.Surface.Friction * 1.25f;

			if ( SurfaceFriction > 1f )
				SurfaceFriction = 1f;

			if ( GroundEntity != null )
			{
				BaseVelocity = GroundEntity.Velocity;

				if ( !wasOnGround )
				{
					var fallVelocity = PreVelocity.z + Gravity;
					var threshold = -FallDamageThreshold;

					if ( fallVelocity < threshold && ( !IsJetpacking || Player.Energy < Player.MaxEnergy * 0.1f ) )
					{
						var overstep = threshold - fallVelocity;
						var fraction = overstep.Remap( 0f, FallDamageThreshold, 0f, 1f ).Clamp( 0f, 1f );

						Pawn.PlaySound( $"player.fall{Rand.Int(1, 3)}" )
							.SetVolume( 0.7f + (0.3f * fraction) )
							.SetPitch( 1f - (0.35f * fraction) );

						OnTakeFallDamage( fraction );
					}
					else
					{
						var volume = Velocity.Length.Remap( 0f, MaxSpeed, 0.1f, 0.5f );
						Pawn.PlaySound( $"player.land{Rand.Int( 1, 4 )}" ).SetVolume( volume );
					}
			 	}
			}
		}

		public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, Mins, Maxs, liftFeet );
		}

		private void OnTakeFallDamage( float fraction )
		{
			if ( Host.IsServer )
			{
				var damage = new DamageInfo()
					.WithAttacker( Pawn )
					.WithFlag( DamageFlags.Fall )
					.WithForce( Vector3.Down * Velocity.Length * fraction )
					.WithPosition( Position );

				damage.Damage = FallDamageMin + (FallDamageMax - FallDamageMin) * fraction;

				Pawn.TakeDamage( damage );
			}
		}

		private void StayOnGround()
		{
			var start = Position + Vector3.Up * 2;
			var end = Position + Vector3.Down * StepSize;

			var trace = TraceBBox( Position, start );
			start = trace.EndPosition;

			trace = TraceBBox( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > StayOnGroundAngle ) return;

			Position = trace.EndPosition;
		}
	}
}
