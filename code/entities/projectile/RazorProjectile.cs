using Gamelib.Maths;
using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public partial class RazorProjectile : BulletDropProjectile
	{
		public Vector3 UpVelocity { get; set; }
		public float SeekRadius { get; set; }

		protected override Vector3 GetTargetPosition()
		{
			var newPosition = base.GetTargetPosition();

			var targets = Physics.GetEntitiesInSphere( Position, SeekRadius )
				.OfType<Player>()
				.Where( IsValidTarget );

			var target = targets.FirstOrDefault();

			if ( target.IsValid() )
			{
				// Remove some of the velocity we added before and chase the target.
				var targetDirection = (target.WorldSpaceBounds.Center - newPosition).Normal;
				newPosition -= Velocity * 0.75f * Time.Delta;
				newPosition += targetDirection * Velocity.Length * 0.5f * Time.Delta;
			}
			else
			{
				// We have no target so move the projectile up if we should.
				newPosition += UpVelocity * Time.Delta;
				UpVelocity -= UpVelocity * 0.75f * Time.Delta;
			}

			return newPosition;
		}

		private bool IsValidTarget( Player player )
		{
			if ( player.LifeState == LifeState.Dead )
			{
				return false;
			}

			if ( Attacker is Player attacker )
			{
				return attacker.Team != player.Team;
			}

			return true;
		}
	}
}
