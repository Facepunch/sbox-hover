using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class DestroyerProjectile :  BouncingProjectile
	{
		public TimeSince TimeSinceCreated { get; private set; }

		private bool PlayedLandingSound { get; set; }

		public DestroyerProjectile()
        {
			TimeSinceCreated = 0f;
		}

		protected override bool HasHitTarget( TraceResult trace )
		{
			if ( TimeSinceCreated > 1f && trace.Hit )
			{
				return true;
			}

			return base.HasHitTarget( trace );
		}

		protected override void ServerTick()
		{
			if ( !PlayedLandingSound && DestroyTime > 0f )
            {
				var trace = Trace.Ray( Position, Position + Velocity.Normal * 1500f + Vector3.Down * GravityModifier )
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
