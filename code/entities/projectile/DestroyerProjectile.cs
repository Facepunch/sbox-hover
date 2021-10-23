using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class DestroyerProjectile : PhysicsProjectile
	{
		public TimeSince TimeSinceCreated { get; private set; }

		private bool PlayedLandingSound { get; set; }

		public DestroyerProjectile()
        {
			TimeSinceCreated = 0f;
		}

		protected override void OnInitialize()
		{
			PhysicsBody.GravityScale = 0.6f;
			PhysicsBody.LinearDrag = 0f;

			base.OnInitialize();
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( IsServer && eventData.Entity.IsValid() && TimeSinceCreated > 1f )
			{
				DestroyTime = 0f;
			}

			base.OnPhysicsCollision( eventData );
		}

		protected override void ServerTick()
		{
			if ( !PlayedLandingSound && DestroyTime > 0f )
            {
				var trace = Trace.Ray( Position, Position + Velocity.Normal * 1500f )
					.Ignore(this)
					.Run();

				if ( trace.Hit )
                {
					PlaySound( "destroyer.landing" );
					PlayedLandingSound = true;
				}
			}

			base.ServerTick();
		}
	}
}
