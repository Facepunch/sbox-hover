using Gamelib.Maths;
using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class BouncingProjectile : BulletDropProjectile
	{
		public float Bounciness { get; set; } = 1f;

		protected override bool HasHitTarget( TraceResult trace )
		{
			if ( LifeTime.HasValue )
			{
				if ( trace.Hit )
				{
					var reflect = Vector3.Reflect( Velocity.Normal, trace.Normal );

					GravityModifier = 0f;
					Velocity = reflect * Velocity.Length * Bounciness;
				}

				return false;
			}

			return base.HasHitTarget( trace );
		}
	}
}
