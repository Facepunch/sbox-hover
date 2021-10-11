using Sandbox;
using System;

namespace Facepunch.Hover
{
	public class VehicleCamera : Camera
	{
		protected virtual float MinFov => 80.0f;
		protected virtual float MaxFov => 100.0f;
		protected virtual float MaxFovSpeed => 1000.0f;
		protected virtual float FovSmoothingSpeed => 4.0f;
		protected virtual float OrbitCooldown => 0.6f;
		protected virtual float OrbitSmoothingSpeed => 25.0f;
		protected virtual float OrbitReturnSmoothingSpeed => 4.0f;
		protected virtual float MinOrbitPitch => -25.0f;
		protected virtual float MaxOrbitPitch => 70.0f;
		protected virtual float FixedOrbitPitch => 10.0f;
		protected virtual float OrbitHeight => 35.0f;
		protected virtual float OrbitDistance => 150.0f;
		protected virtual float MaxOrbitReturnSpeed => 100.0f;
		protected virtual float MinCarPitch => -60.0f;
		protected virtual float MaxCarPitch => 60.0f;
		protected virtual float FirstPersonPitch => 10.0f;
		protected virtual float CarPitchSmoothingSpeed => 1.0f;
		protected virtual float CollisionRadius => 8.0f;
		protected virtual float ShakeSpeed => 10.0f;
		protected virtual float ShakeSpeedThreshold => 1500.0f;
		protected virtual float ShakeMaxSpeed => 2500.0f;
		protected virtual float ShakeMaxLength => 1.0f;

		private bool IsOrbitEnabled;
		private TimeSince TimeSinceOrbit;
		private Angles OrbitAngles;
		private Rotation OrbitYaw;
		private Rotation OrbitPitch;
		private float CurrentFOV;
		private float VehiclePitch;
		private bool IsFirstPerson;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			IsOrbitEnabled = false;
			TimeSinceOrbit = 0.0f;
			OrbitAngles = Angles.Zero;
			OrbitYaw = Rotation.Identity;
			OrbitPitch = Rotation.Identity;
			CurrentFOV = MinFov;
			VehiclePitch = 0;
			IsFirstPerson = false;

			var car = (pawn as Player)?.Vehicle as VehicleEntity;
			if ( !car.IsValid() ) return;

			OrbitYaw = IsFirstPerson ? Rotation.Identity : Rotation.FromYaw( car.Rotation.Yaw() );
			OrbitPitch = IsFirstPerson ? Rotation.FromPitch( FirstPersonPitch ) : Rotation.Identity;
			OrbitAngles = (OrbitYaw * OrbitPitch).Angles();
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var car = (pawn as Player)?.Vehicle as VehicleEntity;
			if ( !car.IsValid() ) return;

			var body = car.PhysicsBody;
			if ( !body.IsValid() )
				return;

			var speed = car.MovementSpeed;
			var speedAbs = Math.Abs( speed );

			if ( IsOrbitEnabled && TimeSinceOrbit > OrbitCooldown )
				IsOrbitEnabled = false;

			var carRot = car.Rotation;
			VehiclePitch = VehiclePitch.LerpTo( car.Grounded ? carRot.Pitch().Clamp( MinCarPitch, MaxCarPitch ) * (speed < 0.0f ? -1.0f : 1.0f) : 0.0f, Time.Delta * CarPitchSmoothingSpeed );

			if ( IsOrbitEnabled )
			{
				var slerpAmount = Time.Delta * OrbitSmoothingSpeed;

				OrbitYaw = Rotation.Slerp( OrbitYaw, Rotation.From( 0.0f, OrbitAngles.yaw, 0.0f ), slerpAmount );
				OrbitPitch = Rotation.Slerp( OrbitPitch, Rotation.From( OrbitAngles.pitch + VehiclePitch, 0.0f, 0.0f ), slerpAmount );
			}
			else
			{
				if ( IsFirstPerson )
				{
					var targetYaw = 0;
					var targetPitch = FirstPersonPitch;
					var slerpAmount = Time.Delta * OrbitReturnSmoothingSpeed;

					OrbitYaw = Rotation.Slerp( OrbitYaw, Rotation.FromYaw( targetYaw ), slerpAmount );
					OrbitPitch = Rotation.Slerp( OrbitPitch, Rotation.FromPitch( targetPitch ), slerpAmount );
				}
				else
				{
					var targetPitch = FixedOrbitPitch.Clamp( MinOrbitPitch, MaxOrbitPitch );
					var targetYaw = !IsFirstPerson && speed < 0.0f ? carRot.Yaw() + 180.0f : carRot.Yaw();
					var slerpAmount = MaxOrbitReturnSpeed > 0.0f ? Time.Delta * (speedAbs / MaxOrbitReturnSpeed).Clamp( 0.0f, OrbitReturnSmoothingSpeed ) : 1.0f;

					OrbitYaw = Rotation.Slerp( OrbitYaw, Rotation.FromYaw( targetYaw ), slerpAmount );
					OrbitPitch = Rotation.Slerp( OrbitPitch, Rotation.FromPitch( targetPitch + VehiclePitch ), slerpAmount );
				}

				OrbitAngles.pitch = OrbitPitch.Pitch();
				OrbitAngles.yaw = OrbitYaw.Yaw();
				OrbitAngles = OrbitAngles.Normal;
			}

			if ( IsFirstPerson )
			{
				DoFirstPerson();
			}
			else
			{
				DoThirdPerson( car, body );
			}

			CurrentFOV = MaxFovSpeed > 0.0f ? CurrentFOV.LerpTo( MinFov.LerpTo( MaxFov, speedAbs / MaxFovSpeed ), Time.Delta * FovSmoothingSpeed ) : MaxFov;
			FieldOfView = CurrentFOV;

			ApplyShake( speedAbs );
		}

		private void DoFirstPerson()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.Rotation * (OrbitYaw * OrbitPitch);

			Viewer = pawn;
		}

		private void DoThirdPerson( VehicleEntity car, PhysicsBody body )
		{
			Rot = OrbitYaw * OrbitPitch;

			var carPos = car.Position + car.Rotation * (body.LocalMassCenter * car.Scale);
			var startPos = carPos;
			var targetPos = startPos + Rot.Backward * (OrbitDistance * car.Scale) + (Vector3.Up * (OrbitHeight * car.Scale));

			var tr = Trace.Ray( startPos, targetPos )
				.Ignore( car )
				.Radius( Math.Clamp( CollisionRadius * car.Scale, 2.0f, 10.0f ) )
				.WorldOnly()
				.Run();

			Pos = tr.EndPos;

			Viewer = null;
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );

			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var car = (pawn as Player)?.Vehicle as VehicleEntity;
			if ( !car.IsValid() ) return;

			if ( input.Pressed( InputButton.View ) )
			{
				IsFirstPerson = !IsFirstPerson;
				OrbitYaw = IsFirstPerson ? Rotation.Identity : Rotation.FromYaw( car.Rotation.Yaw() );
				OrbitPitch = IsFirstPerson ? Rotation.FromPitch( FirstPersonPitch ) : Rotation.Identity;
				OrbitAngles = (OrbitYaw * OrbitPitch).Angles();
				IsOrbitEnabled = false;
				TimeSinceOrbit = 0.0f;
			}

			if ( (Math.Abs( input.AnalogLook.pitch ) + Math.Abs( input.AnalogLook.yaw )) > 0.0f )
			{
				if ( !IsOrbitEnabled )
				{
					OrbitAngles = (OrbitYaw * OrbitPitch).Angles();
					OrbitAngles = OrbitAngles.Normal;

					OrbitYaw = Rotation.From( 0.0f, OrbitAngles.yaw, 0.0f );
					OrbitPitch = Rotation.From( OrbitAngles.pitch, 0.0f, 0.0f );
				}

				IsOrbitEnabled = true;
				TimeSinceOrbit = 0.0f;

				OrbitAngles.yaw += input.AnalogLook.yaw;
				OrbitAngles.pitch += input.AnalogLook.pitch;
				OrbitAngles = OrbitAngles.Normal;
				OrbitAngles.pitch = OrbitAngles.pitch.Clamp( MinOrbitPitch, MaxOrbitPitch );
			}

			if ( IsFirstPerson )
			{
				input.ViewAngles = (car.Rotation * Rotation.From( OrbitAngles )).Angles();
			}
			else
			{
				input.ViewAngles = IsOrbitEnabled ? OrbitAngles : car.Rotation.Angles();
			}

			input.ViewAngles = input.ViewAngles.Normal;
		}

		private void ApplyShake( float speed )
		{
			if ( speed < ShakeSpeedThreshold )
				return;

			var pos = (Time.Now % MathF.PI) * ShakeSpeed;
			var length = (speed - ShakeSpeedThreshold) / (ShakeMaxSpeed - ShakeSpeedThreshold);
			length = length.Clamp( 0, ShakeMaxLength );

			float x = Noise.Perlin( pos, 0, 0 ) * length;
			float y = Noise.Perlin( pos, 5.0f, 0 ) * length;

			Pos += Rot.Right * x + Rot.Up * y;
			Rot *= Rotation.FromAxis( Vector3.Up, x );
			Rot *= Rotation.FromAxis( Vector3.Right, y );
		}
	}
}
