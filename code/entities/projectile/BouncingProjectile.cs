using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class BouncingProjectile : Projectile
	{
		public string BounceSound { get; set; } = "grenade.bounce";
		public float Bounciness { get; set; } = 0.9f;

		protected override bool HasHitTarget( TraceResult trace )
		{
			if ( LifeTime <= 0f )
			{
				return base.HasHitTarget( trace );
			}

			if ( !trace.Hit ) return false;

			var reflect = Vector3.Reflect( Velocity.Normal, trace.Normal );

			GravityModifier = 0f;
			Velocity = reflect * Velocity.Length * Bounciness;

			if ( string.IsNullOrEmpty( BounceSound ) || Velocity.Length <= 8f )
				return false;

			using ( Prediction.Off() )
			{
				PlaySound( BounceSound );
			}

			return false;

		}
	}
}
