using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Hover
{
	public class MoveController : WalkController
	{
		public float CurrentGroundAngle { get; private set; }

		public MoveController()
		{
			Acceleration = 100f;
		}

		public override float GetWishSpeed()
		{
			return SprintSpeed;
		}

		public override void CheckJumpButton()
		{
			// Do nothing. There's no jumping, only jetpacking.
		}

		public override void Simulate()
		{
			var trace = Trace.Ray( Position, Position + Vector3.Down * StepSize )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.Solid, true )
				.HitLayer( CollisionLayer.GRATE, true )
				.HitLayer( CollisionLayer.PLAYER_CLIP, true )
				.Ignore( Pawn )
				.Run();

			CurrentGroundAngle = Vector3.GetAngle( Velocity.Normal, trace.Normal );

			base.Simulate();
		}

		public override void ApplyFriction( float frictionAmount = 1.0f )
		{
			if ( Input.Down( InputButton.Jump ) )
			{
				if ( CurrentGroundAngle < 100f )
					frictionAmount = 0f;
				else
					frictionAmount = 0.4f;
					
			}

			base.ApplyFriction( frictionAmount );
		}
	}
}
