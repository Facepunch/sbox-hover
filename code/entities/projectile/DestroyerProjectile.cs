using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class DestroyerProjectile : PhysicsProjectile
	{
		public TimeSince TimeSinceCreated { get; private set; }

		public DestroyerProjectile()
        {
			TimeSinceCreated = 0f;
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( IsServer && eventData.Entity.IsValid() && TimeSinceCreated > 0.4f )
			{
				DestroyTime = 0f;
			}

			base.OnPhysicsCollision( eventData );
		}
	}
}
