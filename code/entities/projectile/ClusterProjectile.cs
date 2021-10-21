using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class ClusterProjectile : PhysicsProjectile
	{
		public TimeSince TimeSinceCreated { get; private set; }

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( IsServer && eventData.Entity.IsValid() && TimeSinceCreated > 0.2f )
			{
				DestroyTime = 0f;
			}

			base.OnPhysicsCollision( eventData );
		}
	}
}
