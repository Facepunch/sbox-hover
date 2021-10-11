using Sandbox;
using System;

namespace Facepunch.Hover
{
	public struct VehicleWheel
	{
		private readonly VehicleEntity Parent;
		private float PreviousLength;
		private float CurrentLength;

		public VehicleWheel( VehicleEntity parent )
		{
			Parent = parent;
			PreviousLength = 0;
			CurrentLength = 0;
		}

		public bool Raycast( float length, bool doPhysics, Vector3 offset, ref float wheel, float deltaTime )
		{
			var position = Parent.Position;
			var rotation = Parent.Rotation;

			var wheelAttachPos = position + offset;
			var wheelExtend = wheelAttachPos - rotation.Up * (length * Parent.Scale);

			var trace = Trace.Ray( wheelAttachPos, wheelExtend )
				.Ignore( Parent )
				.Ignore( Parent.Driver )
				.Run();

			wheel = length * trace.Fraction;
			var wheelRadius = (14 * Parent.Scale);

			if ( !doPhysics && VehicleEntity.DebugVehicles )
			{
				var wheelPosition = trace.Hit ? trace.EndPos : wheelExtend;
				wheelPosition += rotation.Up * wheelRadius;

				if ( trace.Hit )
				{
					DebugOverlay.Circle( wheelPosition, rotation * Rotation.FromYaw( 90 ), wheelRadius, Color.Red.WithAlpha( 0.5f ), false );
					DebugOverlay.Line( trace.StartPos, trace.EndPos, Color.Red, 0, false );
				}
				else
				{
					DebugOverlay.Circle( wheelPosition, rotation * Rotation.FromYaw( 90 ), wheelRadius, Color.Green.WithAlpha( 0.5f ), false );
					DebugOverlay.Line( wheelAttachPos, wheelExtend, Color.Green, 0, false );
				}
			}

			if ( !trace.Hit || !doPhysics )
			{
				return trace.Hit;
			}

			var body = Parent.PhysicsBody.SelfOrParent;

			PreviousLength = CurrentLength;
			CurrentLength = (length * Parent.Scale) - trace.Distance;

			var springVelocity = (CurrentLength - PreviousLength) / deltaTime;
			var springForce = body.Mass * 50.0f * CurrentLength;
			var damperForce = body.Mass * (1.5f + (1.0f - trace.Fraction) * 3.0f) * springVelocity;
			var velocity = body.GetVelocityAtPoint( wheelAttachPos );
			var speed = velocity.Length;
			var speedDot = MathF.Abs( speed ) > 0.0f ? MathF.Abs( MathF.Min( Vector3.Dot( velocity, rotation.Up.Normal ) / speed, 0.0f ) ) : 0.0f;
			var speedAlongNormal = speedDot * speed;
			var correctionMultiplier = (1.0f - trace.Fraction) * (speedAlongNormal / 1000.0f);
			var correctionForce = correctionMultiplier * 50.0f * speedAlongNormal / deltaTime;

			body.ApplyImpulseAt( wheelAttachPos, trace.Normal * (springForce + damperForce + correctionForce) * deltaTime );

			return true;
		}
	}

}
