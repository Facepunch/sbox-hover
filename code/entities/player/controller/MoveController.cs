using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class MoveController : BasePlayerController
	{
		[Net, Predicted] public float Jetpack { get; set; }
		[Net] public bool IsJetpacking { get; set; }
		public TimeSince LastSkiTime { get; set; }

		public float MaxSpeed { get; set; } = 1000.0f;
		public float JetpackBoost { get; set; } = 120.0f;
		public float SprintSpeed { get; set; } = 400.0f;
		public float DefaultSpeed { get; set; } = 250.0f;
		public float Acceleration { get; set; } = 10.0f;
		public float AirAcceleration { get; set; } = 50.0f;
		public float GroundFriction { get; set; } = 4.0f;
		public float StopSpeed { get; set; } = 100.0f;
		public float GroundAngle { get; set; } = 46.0f;
		public float StepSize { get; set; } = 18.0f;
		public float MaxNonJumpVelocity { get; set; } = 140.0f;
		public float BodyGirth { get; set; } = 32.0f;
		public float BodyHeight { get; set; } = 72.0f;
		public float EyeHeight { get; set; } = 64.0f;
		public float Gravity { get; set; } = 800.0f;
		public float AirControl { get; set; } = 100.0f;
		public bool Swimming { get; set; } = false;

		protected Unstuck Unstuck { get; private set; }
		protected Duck Duck { get; private set; }

		protected float SurfaceFriction { get; set; }
		protected Vector3 Mins { get; set; }
		protected Vector3 Maxs { get; set; }
		protected bool IsTouchingLadder { get; set; }
		protected Vector3 LadderNormal { get; set; }

		public MoveController()
		{
			Duck = new Duck( this );
			Unstuck = new Unstuck( this );
			Jetpack = 100f;
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
			var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
			var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

			Duck.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

			SetBBox( mins, maxs );
		}

		public override void Simulate()
		{
			EyePosLocal = Vector3.Up * (EyeHeight * Pawn.Scale);
			UpdateBBox();

			EyePosLocal += TraceOffset;
			EyeRot = Input.Rotation;

			if ( Unstuck.TestAndFix() )
				return;

			CheckLadder();
			Swimming = Pawn.WaterLevel.Fraction > 0.6f;

			if ( !Swimming && !IsTouchingLadder && !IsJetpacking )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;
				BaseVelocity = BaseVelocity.WithZ( 0 );
			}

			IsJetpacking = false;

			if ( Input.Down( InputButton.Attack2 ) )
			{
				DoJetpackMovement();
			}

			if ( !IsJetpacking )
			{
				Jetpack = (Jetpack + 20f * Time.Delta).Clamp( 0f, 100f );
			}

			var startOnGround = GroundEntity != null;

			if ( startOnGround )
			{
				Velocity = Velocity.WithZ( 0 );

				if ( GroundEntity != null )
				{
					if ( Input.Down( InputButton.Jump ) )
					{
						HandleSki();
					}
					else
					{
						var skiFriction = MathF.Min( (LastSkiTime / 3f), 1f );
						ApplyFriction( GroundFriction * SurfaceFriction * skiFriction );
					}
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

			Duck.PreTick();

			bool stayOnGround = false;
			if ( Swimming )
			{
				ApplyFriction( 1 );
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
				AirMove();
			}

			CategorizePosition( stayOnGround );

			if ( !Swimming && !IsTouchingLadder && !IsJetpacking )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			}

			if ( GroundEntity != null )
			{
				Velocity = Velocity.WithZ( 0 );
			}
		}

		private float GetWishSpeed()
		{
			var ws = Duck.GetWishSpeed();
			if ( ws >= 0 ) return ws;

			if ( Input.Down( InputButton.Run ) )
				return SprintSpeed;

			return DefaultSpeed;
		}

		private void WalkMove()
		{
			var wishDir = WishVelocity.Normal;
			var wishSpeed = WishVelocity.Length;

			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.Normal * wishSpeed;

			Velocity = Velocity.WithZ( 0 );

			Accelerate( wishDir, wishSpeed, 0, Acceleration );

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
					Position = pm.EndPos;
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
			mover.MaxStandableAngle = GroundAngle;

			mover.TryMoveWithStep( Time.Delta, StepSize );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		private void Move()
		{
			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
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

			Velocity += wishDir * accelSpeed;
		}

		private void HandleSki()
		{
			var groundAngle = GetSlopeAngle();

			if ( groundAngle < 100f )
			{
				if ( groundAngle < 85f && Velocity.Length < MaxSpeed )
				{
					Velocity += (Velocity * Time.Delta * 0.3f);
				}
			}
			else
			{
				Velocity *= 0.98f;
			}

			LastSkiTime = 0f;
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
				if ( Jetpack >= 5f )
				{
					IsJetpacking = true;
					Velocity = Velocity.WithZ( startZ + JetpackBoost * Time.Delta );
				}

				Jetpack = (Jetpack - 20f * Time.Delta).Clamp( 0f, 100f );

				return;
			}

			ClearGroundEntity();

			float groundFactor = 1.0f;
			float multiplier = 268.3281572999747f * 1.2f;

			if ( Duck.IsActive )
				multiplier *= 0.8f;

			Velocity = Velocity.WithZ( startZ + multiplier * groundFactor );
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

			AddEvent( "jump" );
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

			Accelerate( wishDir, wishSpeed, 100, Acceleration );

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

			var point = Position - Vector3.Up * 2;
			var bumpOrigin = Position;
			var movingUpRapidly = Velocity.z > MaxNonJumpVelocity;
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

			if ( movingUpRapidly || Swimming )
			{
				ClearGroundEntity();
				return;
			}

			var pm = TraceBBox( bumpOrigin, point, 4.0f );

			if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
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
				Position = pm.EndPos;
			}
		}

		private void UpdateGroundEntity( TraceResult tr )
		{
			GroundNormal = tr.Normal;
			SurfaceFriction = tr.Surface.Friction * 1.25f;
			if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

			GroundEntity = tr.Entity;

			if ( GroundEntity != null )
			{
				BaseVelocity = GroundEntity.Velocity;
			}
		}

		private void ClearGroundEntity()
		{
			if ( GroundEntity == null ) return;

			GroundEntity = null;
			GroundNormal = Vector3.Up;
			SurfaceFriction = 1.0f;
		}

		public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, Mins, Maxs, liftFeet );
		}

		private void StayOnGround()
		{
			var start = Position + Vector3.Up * 2;
			var end = Position + Vector3.Down * StepSize;

			var trace = TraceBBox( Position, start );
			start = trace.EndPos;

			trace = TraceBBox( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

			Position = trace.EndPos;
		}

		private float GetSlopeAngle()
		{
			var trace = Trace.Ray( Position, Position + Vector3.Down * StepSize )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.Solid, true )
				.HitLayer( CollisionLayer.GRATE, true )
				.HitLayer( CollisionLayer.PLAYER_CLIP, true )
				.Ignore( Pawn )
				.Run();

			return Vector3.GetAngle( Velocity.Normal, trace.Normal );
		}
	}
}
