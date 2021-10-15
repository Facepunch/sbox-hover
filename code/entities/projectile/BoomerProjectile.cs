﻿using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class BoomerProjectile : PhysicsProjectile
	{
		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( IsServer && eventData.Entity.IsValid() )
			{
				DestroyTime = 0f;
			}

			base.OnPhysicsCollision( eventData );
		}
	}
}