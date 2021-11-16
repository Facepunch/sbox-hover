using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class ClusterProjectile : BulletDropProjectile
	{
		public TimeSince TimeSinceCreated { get; private set; }

		protected override bool HasHitTarget( TraceResult trace )
		{
			if ( TimeSinceCreated < 0.2f )
			{
				return false;
			}

			return base.HasHitTarget( trace );
		}
	}
}
