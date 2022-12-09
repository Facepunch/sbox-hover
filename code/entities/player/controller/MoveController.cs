using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class MoveController : BaseNetworkable
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

		public Vector3 WishVelocity { get; private set; }

		protected HashSet<string> Events { get; set; } = new();
		protected HashSet<string> Tags { get; set; } = new();

		protected float SurfaceFriction { get; set; }
		protected Vector3 PreVelocity { get; set; }
		protected Vector3 Mins { get; set; }
		protected Vector3 Maxs { get; set; }
		protected bool IsTouchingLadder { get; set; }
		protected Vector3 LadderNormal { get; set; }
		protected Vector3 GroundNormal { get; set; }
		protected Vector3 TraceOffset { get; set; }
		protected int StuckTries { get; set; }

		public HoverPlayer Player { get; set; }

		public void SetActivePlayer( HoverPlayer player )
		{
			Player = player;
		}

		public bool HasEvent( string eventName )
		{
			if ( Events == null ) return false;
			return Events.Contains( eventName );
		}

		public bool HasTag( string tagName )
		{
			if ( Tags == null ) return false;
			return Tags.Contains( tagName );
		}

		public void AddEvent( string eventName )
		{
			if ( Events == null )
				Events = new HashSet<string>();

			if ( Events.Contains( eventName ) )
				return;

			Events.Add( eventName );
		}

		public void SetTag( string tagName )
		{
			Tags ??= new HashSet<string>();

			if ( Tags.Contains( tagName ) )
				return;

			Tags.Add( tagName );
		}

		public void ClearGroundEntity()
		{
			if ( !Player.GroundEntity.IsValid() )
				return;

			Player.GroundEntity = null;
			GroundNormal = Vector3.Up;
			SurfaceFriction = 1.0f;
		}

		public virtual BBox GetHull()
		{
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, BodyHeight );
			return new BBox( mins, maxs );
		}

		public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				maxs = maxs.WithZ( maxs.z - liftFeet );
			}

			var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
				.Size( mins, maxs )
				.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
				.Ignore( Player )
				.Run();

			tr.EndPosition -= TraceOffset;
			return tr;
		}

		public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, Mins, Maxs, liftFeet );
		}

		public virtual void FrameSimulate()
		{
			Assert.NotNull( Player );

			Player.EyeRotation = Player.ViewAngles.ToRotation();
		}

		public virtual void Simulate()
		{
			Assert.NotNull( Player );

			Events?.Clear();
			Tags?.Clear();

			Player.EyeLocalPosition = Vector3.Up * Scale( EyeHeight );
			UpdateBBox();

			Player.EyeLocalPosition += TraceOffset;
			Player.EyeRotation = Player.ViewAngles.ToRotation();

			if ( CheckStuckAndFix() )
			{
				// I hope this never really happens.
				return;
			}

			var tr = TraceBBox( Player.Position, Player.Position + Vector3.Down * 8f, 16f );

			if ( tr.Hit )
			{
				UpdateGroundEntity( tr );
			}

			if ( Impulse.Length > 0 )
			{
				ClearGroundEntity();
				Player.Velocity += Impulse;
				Impulse = 0f;
			}

			CheckLadder();
			Swimming = Player.WaterLevel > 0.6f;

			PreVelocity = Player.Velocity;

			if ( !Swimming && !IsTouchingLadder && !IsJetpacking )
			{
				Player.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
				Player.Velocity += new Vector3( 0, 0, Player.BaseVelocity.z ) * Time.Delta;
				Player.BaseVelocity = Player.BaseVelocity.WithZ( 0 );
			}

			IsJetpacking = false;
			IsSkiing = false;

			if ( Input.Down( Input.UsingController ? InputButton.Jump : InputButton.SecondaryAttack ) )
			{
				DoJetpackMovement();
			}

			var startOnGround = Player.GroundEntity.IsValid();

			if ( startOnGround )
			{
				Player.Velocity = Player.Velocity.WithZ( 0 );

				if ( Input.Down( Input.UsingController ? InputButton.SecondaryAttack : InputButton.Jump ) )
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

			WishVelocity = new Vector3( Player.InputDirection.x, Player.InputDirection.y, 0 );
			var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
			WishVelocity *= Player.ViewAngles.ToRotation();

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
			else if ( Player.GroundEntity.IsValid() )
			{
				stayOnGround = true;
				WalkMove();
			}
			else
			{
				Player.Velocity -= Player.Velocity * 0.05f * Time.Delta;
				AirMove();
			}

			if ( !IsJetpacking )
				CategorizePosition( stayOnGround );
			else
				ClearGroundEntity();

			if ( !Swimming && !IsTouchingLadder && !IsJetpacking )
			{
				Player.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			}

			if ( Player.GroundEntity.IsValid() )
			{
				Player.Velocity = Player.Velocity.WithZ( 0 );
			}

			if ( IsSkiing ) SetTag( "skiing" );
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

			Player.Velocity = Player.Velocity.WithZ( 0 );

			Accelerate( wishDir, wishSpeed, 0f, Acceleration );

			if ( IsSkiing && !IsJetpacking )
			{
				var trace = Trace.Ray( Player.Position, Player.Position + Vector3.Down * 100f )
					.Ignore( Player )
					.Run();

				var direction = Player.Velocity.WithZ( 0f ).Normal;
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

				Player.Velocity += trace.Normal.WithZ( 0f ) * Player.Velocity.Length * scale * Time.Delta;
				Player.Velocity = Player.Velocity.ClampLength( MaxSpeed );
			}

			Player.Velocity = Player.Velocity.WithZ( 0 );
			Player.Velocity += Player.BaseVelocity;

			try
			{
				if ( Player.Velocity.Length < 1.0f )
				{
					Player.Velocity = Vector3.Zero;
					return;
				}

				var dest = (Player.Position + Player.Velocity * Time.Delta).WithZ( Player.Position.z );
				var pm = TraceBBox( Player.Position, dest );

				if ( pm.Fraction == 1 )
				{
					Player.Position = pm.EndPosition;
					StayOnGround();
					return;
				}

				StepMove();
			}
			finally
			{
				Player.Velocity -= Player.BaseVelocity;
			}

			StayOnGround();
		}

		private void StepMove()
		{
			var mover = new MoveHelper( Player.Position, Player.Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Player );
			mover.WallBounce = IsSkiing ? SkiWallBounce : 0f;
			mover.MaxStandableAngle = GroundAngle;
			mover.TryMoveWithStep( Time.Delta, StepSize );

			Player.Position = mover.Position;
			Player.Velocity = mover.Velocity;
		}

		private void Move()
		{
			var mover = new MoveHelper( Player.Position, Player.Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Player );
			mover.WallBounce = IsJetpacking ? JetpackSurfaceBounce : 0f;
			mover.MaxStandableAngle = GroundAngle;
			mover.TryMove( Time.Delta );

			Player.Position = mover.Position;
			Player.Velocity = mover.Velocity;
		}

		private void Accelerate( Vector3 wishDir, float wishSpeed, float speedLimit, float acceleration )
		{
			if ( speedLimit > 0 && wishSpeed > speedLimit )
				wishSpeed = speedLimit;

			var currentSpeed = Player.Velocity.Dot( wishDir );
			var addSpeed = wishSpeed - currentSpeed;

			if ( addSpeed <= 0 )
				return;

			var accelSpeed = acceleration * Time.Delta * wishSpeed * SurfaceFriction;

			if ( accelSpeed > addSpeed )
				accelSpeed = addSpeed;

			if ( IsSkiing || LastSkiTime < 0.5f )
			{
				var previousLength = Math.Max( Player.Velocity.Length, wishSpeed );
				Player.Velocity += wishDir * accelSpeed;
				Player.Velocity = Player.Velocity.ClampLength( previousLength );
			}
			else
			{
				Player.Velocity += wishDir * accelSpeed;
			}
		}

		private void ClassicAccelerate( Vector3 wishDir, float wishSpeed, float speedLimit )
		{
			if ( speedLimit > 0 && wishSpeed > speedLimit )
				wishSpeed = speedLimit;

			var wishVelocity = wishDir * wishSpeed;
			var pushDir = wishVelocity - Player.Velocity;
			var pushLen = pushDir.Length;
			var canPush = 1f * Time.Delta * wishSpeed;
			
			if ( canPush > pushLen )
				canPush = pushLen;

			Player.Velocity += (pushDir * canPush * Time.Delta);
		}

		private void HandleSki()
		{
			LastSkiTime = 0f;
			IsSkiing = true;
		}

		private bool CheckStuckAndFix()
		{
			var result = TraceBBox( Player.Position, Player.Position );

			if ( !result.StartedSolid )
			{
				StuckTries = 0;
				return false;
			}

			if ( Host.IsClient ) return true;

			var attemptsPerTick = 20;

			for ( int i = 0; i < attemptsPerTick; i++ )
			{
				var pos = Player.Position + Vector3.Random.Normal * (StuckTries / 2.0f);

				if ( i == 0 )
				{
					pos = Player.Position + Vector3.Up * 5;
				}

				result = TraceBBox( pos, pos );

				if ( !result.StartedSolid )
				{
					Player.Position = pos;
					return false;
				}
			}

			StuckTries++;
			return true;
		}

		private void ApplyFriction( float frictionAmount = 1.0f )
		{
			var speed = Player.Velocity.Length;
			if ( speed < 0.1f ) return;

			var control = (speed < StopSpeed) ? StopSpeed : speed;
			var drop = control * Time.Delta * frictionAmount;
			var newSpeed = speed - drop;

			if ( newSpeed < 0 ) newSpeed = 0;

			if ( newSpeed != speed )
			{
				newSpeed /= speed;
				Player.Velocity *= newSpeed;
			}
		}

		private void DoJetpackMovement()
		{
			if ( Swimming )
			{
				ClearGroundEntity();
				Player.Velocity = Player.Velocity.WithZ( 100 );
				return;
			}

			var startZ = Player.Velocity.z;

			if ( !Player.GroundEntity.IsValid() )
			{
				if ( Player.Energy >= 5f )
				{
					IsJetpacking = true;

					Player.Velocity += Player.Velocity.WithZ( 0f ).Normal * Scale( JetpackAimThrust * JetpackScale ) * Time.Delta;

					if ( Player.Velocity.z <= MaxJetpackVelocity )
					{
						Player.Velocity += Vector3.Up * Gravity * Time.Delta;

						var boost = Scale( JetpackBoost ) * JetpackScale * 2f;
						var trace = Trace.Ray( Player.Position, Vector3.Up * boost * Time.Delta * 8f )
							.Ignore( Player )
							.Run();

						if ( !trace.Hit )
                        {
							Player.Velocity += Vector3.Up * boost * Time.Delta;
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

				Player.Velocity = Player.Velocity.WithZ( startZ + multiplier * groundFactor );
				Player.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

				AddEvent( "jump" );
			}
		}

		private float Scale( float speed )
		{
			return speed * Player.Scale;
		}

		private Vector3 Scale( Vector3 velocity )
		{
			return velocity * Player.Scale;
		}

		private void AirMove()
		{
			var wishDir = WishVelocity.Normal;
			var wishSpeed = WishVelocity.Length;

			Accelerate( wishDir, wishSpeed, AirControl, AirAcceleration );

			Player.Velocity += Player.BaseVelocity;

			Move();

			Player.Velocity -= Player.BaseVelocity;
		}

		private void WaterMove()
		{
			var wishDir = WishVelocity.Normal;
			var wishSpeed = WishVelocity.Length;

			wishSpeed *= 0.8f;

			Accelerate( wishDir, wishSpeed, 100f, Acceleration );

			Player.Velocity += Player.BaseVelocity;

			Move();

			Player.Velocity -= Player.BaseVelocity;
		}

		private void CheckLadder()
		{
			if ( IsTouchingLadder && Input.Pressed( InputButton.Jump ) )
			{
				Player.Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;
				return;
			}

			var ladderDistance = 1.0f;
			var start = Player.Position;
			var end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : WishVelocity.Normal) * ladderDistance;

			var pm = Trace.Ray( start, end )
				.Size( Mins, Maxs )
				.WithTag( "ladder" )
				.Ignore( Player )
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

			Player.Velocity = (velocity - cross) + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

			Move();
		}

		private void CategorizePosition( bool stayOnGround )
		{
			SurfaceFriction = 1.0f;

			var point = Player.Position - Vector3.Up * 2f;
			var bumpOrigin = Player.Position;
			var moveToEndPos = false;

			if ( Player.GroundEntity.IsValid() )
			{
				moveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( stayOnGround )
			{
				moveToEndPos = true;
				point.z -= StepSize;
			}

			if ( Player.Velocity.z > MaxNonJumpVelocity || Swimming )
			{
				ClearGroundEntity();
				return;
			}

			var pm = TraceBBox( bumpOrigin, point, 16f );

			if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > StayOnGroundAngle )
			{
				ClearGroundEntity();
				moveToEndPos = false;

				if ( Player.Velocity.z > 0 )
					SurfaceFriction = 0.25f;
			}
			else
			{
				UpdateGroundEntity( pm );
			}

			if ( moveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				Player.Position = pm.EndPosition;
			}
		}

		private void UpdateGroundEntity( TraceResult trace )
		{
			var wasOnGround = Player.GroundEntity.IsValid();

			Player.GroundEntity = trace.Entity;
			SurfaceFriction = trace.Surface.Friction * 1.25f;
			GroundNormal = trace.Normal;

			if ( SurfaceFriction > 1f )
				SurfaceFriction = 1f;

			if ( Player.GroundEntity.IsValid() )
			{
				Player.BaseVelocity = Player.GroundEntity.Velocity;

				if ( !wasOnGround )
				{
					var fallVelocity = PreVelocity.z + Gravity;
					var threshold = -FallDamageThreshold;

					if ( fallVelocity < threshold && ( !IsJetpacking || Player.Energy < Player.MaxEnergy * 0.1f ) )
					{
						var overstep = threshold - fallVelocity;
						var fraction = overstep.Remap( 0f, FallDamageThreshold, 0f, 1f ).Clamp( 0f, 1f );

						Player.PlaySound( $"player.fall{Rand.Int(1, 3)}" )
							.SetVolume( 0.7f + (0.3f * fraction) )
							.SetPitch( 1f - (0.35f * fraction) );

						OnTakeFallDamage( fraction );
					}
					else
					{
						var volume = Player.Velocity.Length.Remap( 0f, MaxSpeed, 0.1f, 0.5f );
						Player.PlaySound( $"player.land{Rand.Int( 1, 4 )}" ).SetVolume( volume );
					}
			 	}
			}
		}

		private void OnTakeFallDamage( float fraction )
		{
			if ( Host.IsServer )
			{
				var damage = new DamageInfo()
					.WithAttacker( Player )
					.WithFlag( DamageFlags.Fall )
					.WithForce( Vector3.Down * Player.Velocity.Length * fraction )
					.WithPosition( Player.Position );

				damage.Damage = FallDamageMin + (FallDamageMax - FallDamageMin) * fraction;

				Player.TakeDamage( damage );
			}
		}

		private void StayOnGround()
		{
			var start = Player.Position + Vector3.Up * 2;
			var end = Player.Position + Vector3.Down * StepSize;

			var trace = TraceBBox( Player.Position, start );
			start = trace.EndPosition;

			trace = TraceBBox( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > StayOnGroundAngle ) return;

			Player.Position = trace.EndPosition;
		}
	}
}
