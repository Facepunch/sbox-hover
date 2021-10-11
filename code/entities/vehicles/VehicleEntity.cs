using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class VehicleEntity : Prop, IUse
	{
		[ConVar.Replicated( "debug_vehicles" )]
		public static bool DebugVehicles { get; set; } = false;
		public static float AccelerationSpeed { get; set; } = 500f;

		private VehicleWheel FrontLeftWheel;
		private VehicleWheel FrontRightWheel;
		private VehicleWheel BackLeftWheel;
		private VehicleWheel BackRightWheel;

		private float FrontLeftDistance;
		private float FrontRightDistance;
		private float BackLeftDistance;
		private float BackRightDistance;

		private bool FrontWheelsOnGround;
		private bool BackWheelsOnGround;
		private float AccelerateDirection;
		private float AirRoll;
		private float AirTilt;
		private float Grip;
		private TimeSince DriverEnteredTime;
		private TimeSince DriverLeftTime;

		[Net] private float WheelSpeed { get; set; }
		[Net] private float TurnDirection { get; set; }
		[Net] private float AccelerationTilt { get; set; }
		[Net] private float TurnLean { get; set; }
		[Net] public float MovementSpeed { get; private set; }
		[Net] public bool Grounded { get; private set; }

		private struct InputState
		{
			public float Throttle;
			public float Turning;
			public float Braking;
			public float Tilt;
			public float Roll;

			public void Reset()
			{
				Throttle = 0;
				Turning = 0;
				Braking = 0;
				Tilt = 0;
				Roll = 0;
			}
		}

		private InputState CurrentInput;

		public VehicleEntity()
		{
			FrontLeftWheel = new VehicleWheel( this );
			FrontRightWheel = new VehicleWheel( this );
			BackLeftWheel = new VehicleWheel( this );
			BackRightWheel = new VehicleWheel( this );
		}

		[Net] public Player Driver { get; private set; }

		private ModelEntity _wheelModel0;
		private ModelEntity _wheelModel1;
		private ModelEntity _wheelModel2;
		private ModelEntity _wheelModel3;

		public override void Spawn()
		{
			base.Spawn();

			var modelName = "models/vehicles/skidmark/skidmark.vmdl";

			SetModel( modelName );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
			SetInteractsExclude( CollisionLayer.Player );
			EnableSelfCollisions = false;

			var trigger = new ModelEntity
			{
				Parent = this,
				Position = Position,
				Rotation = Rotation,
				EnableTouch = true,
				CollisionGroup = CollisionGroup.Trigger,
				Transmit = TransmitType.Never,
				EnableSelfCollisions = false,
			};

			trigger.SetModel( modelName );
			trigger.SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( Driver is Player player )
			{
				RemoveDriver( player );
			}
		}

		public void ResetInput()
		{
			CurrentInput.Reset();
		}

		[Event.Tick.Server]
		protected void Tick()
		{
			if ( Driver is Player player )
			{
				if ( player.LifeState != LifeState.Alive || player.Vehicle != this )
				{
					RemoveDriver( player );
				}
			}
		}

		public override void Simulate( Client owner )
		{
			if ( owner == null ) return;
			if ( !IsServer ) return;

			using ( Prediction.Off() )
			{
				CurrentInput.Reset();

				if ( DriverEnteredTime > 1f && Input.Pressed( InputButton.Use ) )
				{
					if ( owner.Pawn is Player player )
					{
						RemoveDriver( player );

						return;
					}
				}

				CurrentInput.Throttle = (Input.Down( InputButton.Forward ) ? 1 : 0) + (Input.Down( InputButton.Back ) ? -1 : 0);
				CurrentInput.Turning = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
				CurrentInput.Braking = (Input.Down( InputButton.Jump ) ? 1 : 0);
				CurrentInput.Tilt = (Input.Down( InputButton.Run ) ? 1 : 0) + (Input.Down( InputButton.Duck ) ? -1 : 0);
				CurrentInput.Roll = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
			}
		}

		[Event.Physics.PreStep]
		public void OnPrePhysicsStep()
		{
			if ( !IsServer )
				return;

			var selfBody = PhysicsBody;
			if ( !selfBody.IsValid() )
				return;

			var body = selfBody.SelfOrParent;
			if ( !body.IsValid() )
				return;

			var deltaTime = Time.Delta;

			body.DragEnabled = false;

			var rotation = selfBody.Rotation;

			AccelerateDirection = CurrentInput.Throttle.Clamp( -1, 1 ) * (1f - CurrentInput.Braking);
			TurnDirection = TurnDirection.LerpTo( CurrentInput.Turning.Clamp( -1, 1 ), 1f - MathF.Pow( 0.001f, deltaTime ) );

			AirRoll = AirRoll.LerpTo( CurrentInput.Roll.Clamp( -1, 1 ), 1f - MathF.Pow( 0.0001f, deltaTime ) );
			AirTilt = AirTilt.LerpTo( CurrentInput.Tilt.Clamp( -1, 1 ), 1f - MathF.Pow( 0.0001f, deltaTime ) );

			var targetTilt = 0f;
			var targetLean = 0f;

			var localVelocity = rotation.Inverse * body.Velocity;

			if ( BackWheelsOnGround || FrontWheelsOnGround )
			{
				var forwardSpeed = MathF.Abs( localVelocity.x );
				var speedFraction = MathF.Min( forwardSpeed / 500f, 1 );

				targetTilt = AccelerateDirection.Clamp( -1f, 1f );
				targetLean = speedFraction * TurnDirection;
			}

			AccelerationTilt = AccelerationTilt.LerpTo( targetTilt, 1f - MathF.Pow( 0.01f, deltaTime ) );
			TurnLean = TurnLean.LerpTo( targetLean, 1f - MathF.Pow( 0.01f, deltaTime ) );

			if ( BackWheelsOnGround )
			{
				var forwardSpeed = MathF.Abs( localVelocity.x );
				var speedFactor = 1f - (forwardSpeed / 5000f).Clamp( 0f, 1f );
				var acceleration = speedFactor * (AccelerateDirection < 0f ? AccelerationSpeed * 0.5f : AccelerationSpeed) * AccelerateDirection * deltaTime;
				var impulse = rotation * new Vector3( acceleration, 0, 0 );
				body.Velocity += impulse;
			}

			RaycastWheels( rotation, true, out FrontWheelsOnGround, out BackWheelsOnGround, deltaTime );
			var onGround = FrontWheelsOnGround || BackWheelsOnGround;
			var fullyGrounded = (FrontWheelsOnGround && BackWheelsOnGround);
			Grounded = onGround;

			if ( fullyGrounded )
			{
				body.Velocity += PhysicsWorld.Gravity * deltaTime;
			}

			body.GravityScale = fullyGrounded ? 0 : 1;

			var canAirControl = false;
			var velocity = rotation * localVelocity.WithZ( 0 );
			var vDelta = MathF.Pow( (velocity.Length / 1000f).Clamp( 0, 1 ), 5f ).Clamp( 0, 1 );

			if ( vDelta < 0.01f ) vDelta = 0;

			if ( DebugVehicles )
			{
				DebugOverlay.Line( body.MassCenter, body.MassCenter + rotation.Forward.Normal * 100, Color.White, 0, false );
				DebugOverlay.Line( body.MassCenter, body.MassCenter + velocity.Normal * 100, Color.Green, 0, false );
			}

			var angle = (rotation.Forward.Normal * MathF.Sign( localVelocity.x )).Normal.Dot( velocity.Normal ).Clamp( 0f, 1f );
			angle = angle.LerpTo( 1f, 1f - vDelta );
			Grip = Grip.LerpTo( angle, 1f - MathF.Pow( 0.001f, deltaTime ) );

			if ( DebugVehicles )
			{
				DebugOverlay.ScreenText( new Vector2( 200, 200 ), $"{Grip}" );
			}

			var angularDamping = 0f;
			angularDamping = angularDamping.LerpTo( 5f, Grip );

			body.LinearDamping = 0f;
			body.AngularDamping = fullyGrounded ? angularDamping : 0.5f;

			if ( onGround )
			{
				localVelocity = rotation.Inverse * body.Velocity;
				WheelSpeed = localVelocity.x;
				var turnAmount = FrontWheelsOnGround ? (MathF.Sign( localVelocity.x ) * 25f * CalculateTurnFactor( TurnDirection, MathF.Abs( localVelocity.x ) ) * deltaTime) : 0f;
				body.AngularVelocity += rotation * new Vector3( 0, 0, turnAmount );

				AirRoll = 0f;
				AirTilt = 0f;

				var forwardGrip = 0.1f;
				forwardGrip = forwardGrip.LerpTo( 0.9f, CurrentInput.Braking );
				body.Velocity = VelocityDamping( Velocity, rotation, new Vector3( forwardGrip, Grip, 0f ), deltaTime );
			}
			else
			{
				var position = selfBody.Position + (rotation * selfBody.LocalMassCenter);
				var trace = Trace.Ray( position, position + rotation.Down * 50f )
					.Ignore( this )
					.Run();

				if ( DebugVehicles )
					DebugOverlay.Line( trace.StartPos, trace.EndPos, trace.Hit ? Color.Red : Color.Green );

				canAirControl = !trace.Hit;
			}

			if ( canAirControl && (AirRoll != 0 || AirTilt != 0) )
			{
				var offset = 50f * Scale;
				var position = selfBody.Position + (rotation * selfBody.LocalMassCenter) + (rotation.Right * AirRoll * offset) + (rotation.Down * (10 * Scale));
				var trace = Trace.Ray( position, position + rotation.Up * (25 * Scale) )
					.Ignore( this )
					.Run();

				if ( DebugVehicles )
					DebugOverlay.Line( trace.StartPos, trace.EndPos );

				bool dampen = false;

				if ( CurrentInput.Roll.Clamp( -1, 1 ) != 0 )
				{
					var force = trace.Hit ? 400f : 100f;
					var roll = trace.Hit ? CurrentInput.Roll.Clamp( -1, 1 ) : AirRoll;
					body.ApplyForceAt( selfBody.MassCenter + rotation.Left * (offset * roll), (rotation.Down * roll) * (roll * (body.Mass * force)) );

					if ( DebugVehicles )
						DebugOverlay.Sphere( selfBody.MassCenter + rotation.Left * (offset * roll), 8, Color.Red );

					dampen = true;
				}

				if ( !trace.Hit && CurrentInput.Tilt.Clamp( -1, 1 ) != 0 )
				{
					var force = 200f;
					body.ApplyForceAt( selfBody.MassCenter + rotation.Forward * (offset * AirTilt), (rotation.Down * AirTilt) * (AirTilt * (body.Mass * force)) );

					if ( DebugVehicles )
						DebugOverlay.Sphere( selfBody.MassCenter + rotation.Forward * (offset * AirTilt), 8, Color.Green );

					dampen = true;
				}

				if ( dampen )
					body.AngularVelocity = VelocityDamping( body.AngularVelocity, rotation, 0.95f, deltaTime );
			}

			localVelocity = rotation.Inverse * body.Velocity;
			MovementSpeed = localVelocity.x;
		}

		private static float CalculateTurnFactor( float direction, float speed )
		{
			var turnFactor = MathF.Min( speed / 500f, 1 );
			var yawSpeedFactor = 1f - (speed / 1000f).Clamp( 0, 0.6f );

			return direction * turnFactor * yawSpeedFactor;
		}

		private static Vector3 VelocityDamping( Vector3 velocity, Rotation rotation, Vector3 damping, float deltaTime )
		{
			var localVelocity = rotation.Inverse * velocity;
			var dampingPow = new Vector3( MathF.Pow( 1f - damping.x, deltaTime ), MathF.Pow( 1f - damping.y, deltaTime ), MathF.Pow( 1f - damping.z, deltaTime ) );
			return rotation * (localVelocity * dampingPow);
		}

		private void RaycastWheels( Rotation rotation, bool doPhysics, out bool frontWheels, out bool backWheels, float deltaTime )
		{
			var forward = 42f;
			var right = 32f;
			var frontLeftPos = rotation.Forward * forward + rotation.Right * right + rotation.Up * 20;
			var frontRightPos = rotation.Forward * forward - rotation.Right * right + rotation.Up * 20;
			var backLeftPos = -rotation.Forward * forward + rotation.Right * right + rotation.Up * 20;
			var backRightPos = -rotation.Forward * forward - rotation.Right * right + rotation.Up * 20;

			var tiltAmount = AccelerationTilt * 2.5f;
			var leanAmount = TurnLean * 2.5f;

			var length = 20f;

			frontWheels =
				FrontLeftWheel.Raycast( length + tiltAmount - leanAmount, doPhysics, frontLeftPos * Scale, ref FrontLeftDistance, deltaTime ) |
				FrontRightWheel.Raycast( length + tiltAmount + leanAmount, doPhysics, frontRightPos * Scale, ref FrontRightDistance, deltaTime );

			backWheels =
				BackLeftWheel.Raycast( length - tiltAmount - leanAmount, doPhysics, backLeftPos * Scale, ref BackLeftDistance, deltaTime ) |
				BackRightWheel.Raycast( length - tiltAmount + leanAmount, doPhysics, backRightPos * Scale, ref BackRightDistance, deltaTime );
		}

		private float WheelAngle = 0f;
		private float WheelRevolute = 0f;

		[Event.Frame]
		public void OnFrame()
		{
			WheelAngle = WheelAngle.LerpTo( TurnDirection * 25, 1f - MathF.Pow( 0.001f, Time.Delta ) );
			WheelRevolute += (WheelSpeed / (14f * Scale)).RadianToDegree() * Time.Delta;

			var wheelRotRight = Rotation.From( -WheelAngle, 180, -WheelRevolute );
			var wheelRotLeft = Rotation.From( WheelAngle, 0, WheelRevolute );
			var wheelRotBackRight = Rotation.From( 0, 90, -WheelRevolute );
			var wheelRotBackLeft = Rotation.From( 0, -90, WheelRevolute );

			RaycastWheels( Rotation, false, out _, out _, Time.Delta );

			/*
			_wheelModel0.LocalRotation = wheelRotRight;
			_wheelModel1.LocalRotation = wheelRotLeft;
			_wheelModel2.LocalRotation = wheelRotBackRight;
			_wheelModel3.LocalRotation = wheelRotBackLeft;
			*/
		}

		private void RemoveDriver( Player player )
		{
			Driver = null;
			DriverLeftTime = 0;

			ResetInput();

			if ( !player.IsValid() )
				return;

			player.EnableSolidCollisions = true;
			player.Vehicle = null;
			player.Controller = new MoveController();
			player.Animator = new StandardPlayerAnimator();
			player.Camera = new FirstPersonCamera();
			player.Parent = null;

			if ( player.PhysicsBody.IsValid() )
			{
				player.PhysicsBody.Enabled = true;
				player.PhysicsBody.Position = player.Position;
			}
		}

		public bool OnUse( Entity user )
		{
			if ( user is Player player && player.Vehicle == null && DriverLeftTime > 1f )
			{
				player.EnableSolidCollisions = false;
				player.Vehicle = this;
				player.Controller = new VehicleController();
				player.Animator = new VehicleAnimator();
				player.Camera = new VehicleCamera();
				player.Parent = this;
				player.LocalPosition = Vector3.Up * 10;
				player.LocalRotation = Rotation.Identity;
				player.LocalScale = 1;
				player.PhysicsBody.Enabled = false;

				DriverEnteredTime = 0;
				Driver = player;
			}

			return false;
		}

		public bool IsUsable( Entity user )
		{
			return Driver == null;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( !IsServer )
				return;

			var body = PhysicsBody;
			if ( !body.IsValid() )
				return;

			body = body.SelfOrParent;
			if ( !body.IsValid() )
				return;

			if ( other is Player player && player.Vehicle == null )
			{
				var speed = body.Velocity.Length;
				var forceOrigin = Position + Rotation.Down * Rand.Float( 20, 30 );
				var velocity = (player.Position - forceOrigin).Normal * speed;
				var angularVelocity = body.AngularVelocity;

				OnPhysicsCollision( new CollisionEventData
				{
					Entity = player,
					Pos = player.Position + Vector3.Up * 50,
					Velocity = velocity,
					PreVelocity = velocity,
					PostVelocity = velocity,
					PreAngularVelocity = angularVelocity,
					Speed = speed,
				} );
			}
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( !IsServer )
				return;

			if ( eventData.Entity is Player player && player.Vehicle != null )
			{
				return;
			}

			var propData = GetModelPropData();

			var minImpactSpeed = propData.MinImpactDamageSpeed;
			if ( minImpactSpeed <= 0f ) minImpactSpeed = 500;

			var impactDmg = propData.ImpactDamage;
			if ( impactDmg <= 0f ) impactDmg = 10;

			var speed = eventData.Speed;

			if ( speed > minImpactSpeed )
			{
				if ( eventData.Entity.IsValid() && eventData.Entity != this )
				{
					var damage = speed / minImpactSpeed * impactDmg * 1.2f;
					eventData.Entity.TakeDamage( DamageInfo.Generic( damage )
						.WithFlag( DamageFlags.PhysicsImpact )
						.WithFlag( DamageFlags.Vehicle )
						.WithAttacker( Driver != null ? Driver : this, Driver != null ? this : null )
						.WithPosition( eventData.Pos )
						.WithForce( eventData.PreVelocity ) );

					if ( eventData.Entity.LifeState == LifeState.Dead && eventData.Entity is not Player )
					{
						PhysicsBody.Velocity = eventData.PreVelocity;
						PhysicsBody.AngularVelocity = eventData.PreAngularVelocity;
					}
				}
			}
		}
	}
}
