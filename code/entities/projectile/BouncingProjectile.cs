﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class BouncingProjectile : Projectile
	{
		public string BounceSound { get; set; }
		public float Bounciness { get; set; } = 1f;

		protected override bool HasHitTarget( TraceResult trace )
		{
			if ( LifeTime > 0f )
			{
				if ( trace.Hit )
				{
					var reflect = Vector3.Reflect( Velocity.Normal, trace.Normal );

					GravityModifier = 0f;
					Velocity = reflect * Velocity.Length * Bounciness;

					if ( !string.IsNullOrEmpty( BounceSound ) && Velocity.Length > 8f )
					{
						using ( Prediction.Off() )
						{
							PlaySound( BounceSound );
						}
					}
				}

				return false;
			}

			return base.HasHitTarget( trace );
		}
	}
}
