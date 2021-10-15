using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class StickyGrenade : PhysicsProjectile
	{
		private bool IsPrimedToExplode { get; set; }

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( IsServer && eventData.Entity.IsValid() )
			{
				PhysicsEnabled = false;
				SetParent( eventData.Entity );
				PlaySound( "sticky.attach" );
				DestroyTime = 3f;
			}

			base.OnPhysicsCollision( eventData );
		}

		protected override void ServerTick()
		{
			if ( !IsPrimedToExplode && DestroyTime < 1f )
			{
				IsPrimedToExplode = true;
				PlaySound( "sticky.warning" );
			}

			base.ServerTick();
		}
	}
}
