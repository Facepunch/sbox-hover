using Sandbox;

namespace Facepunch.Hover
{
	public partial class SpectateCamera : CameraMode
	{
		public override void Activated()
		{
			base.Activated();

			if ( Local.Pawn is Player player )
			{
				Position = player.EyePosition;
				Rotation = player.EyeRotation;
			}

			FieldOfView = 80f;
		}

		public override void Update()
		{
			if ( Local.Pawn is not Player player )
				return;

			Position = player.EyePosition;

			if ( player.Ragdoll.IsValid() )
			{
				var direction = (player.Ragdoll.PhysicsBody.Position - Position).Normal;
				Rotation = Rotation.Slerp( Rotation, Rotation.LookAt( direction ), Time.Delta );
			}
		}
	}
}
