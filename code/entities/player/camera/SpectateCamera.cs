using Sandbox;

namespace Facepunch.Hover
{
	public partial class SpectateCamera : Camera
	{
		public override void Activated()
		{
			base.Activated();

			if ( Local.Pawn is Player player )
			{
				Pos = player.EyePos;
				Rot = player.EyeRot;
			}

			FieldOfView = 80f;
		}

		public override void Update()
		{
			if ( Local.Pawn is not Player player )
				return;

			Pos = player.EyePos;

			if ( player.Ragdoll.IsValid() )
			{
				var direction = (player.Ragdoll.PhysicsBody.Position - Pos).Normal;
				Rot = Rotation.Slerp( Rot, Rotation.LookAt( direction ), Time.Delta );
			}
		}
	}
}
