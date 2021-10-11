using Sandbox;

namespace Facepunch.Hover
{
	public class VehicleAnimator : PawnAnimator
	{
		public override void Simulate()
		{
			ResetParams();

			SetParam( "b_grounded", true );
			SetParam( "b_sit", true );

			var eyeAngles = (Pawn.Rotation.Inverse * Pawn.EyeRot).Angles();
			eyeAngles.pitch = eyeAngles.pitch.Clamp( -25, 70 );
			eyeAngles.yaw = eyeAngles.yaw.Clamp( -90, 90 );

			var aimPos = Pawn.EyePos + (Pawn.Rotation * Rotation.From( eyeAngles )).Forward * 200f;

			SetLookAt( "aim_eyes", aimPos );
			SetLookAt( "aim_head", aimPos );
			SetLookAt( "aim_body", aimPos );

			if ( Pawn.ActiveChild is BaseCarriable carry )
			{
				carry.SimulateAnimator( this );
			}
			else
			{
				SetParam( "holdtype", 0 );
				SetParam( "aim_body_weight", 0.5f );
			}
		}
	}
}
