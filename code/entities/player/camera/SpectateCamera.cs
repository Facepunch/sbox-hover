using Sandbox;

namespace Facepunch.Hover
{
	public partial class SpectateCamera
	{
		public void Update()
		{
			if ( Local.Pawn is not HoverPlayer player )
				return;

			Camera.Position = player.EyePosition;

			if ( player.Ragdoll.IsValid() )
			{
				var direction = (player.Ragdoll.PhysicsBody.Position - Camera.Position).Normal;
				Camera.Rotation = Rotation.Slerp( Camera.Rotation, Rotation.LookAt( direction ), Time.Delta );
			}
		}
	}
}

