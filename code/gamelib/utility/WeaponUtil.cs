using Gamelib.Extensions;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamelib.Utility
{
	public static partial class WeaponUtil
	{
		[ClientRpc]
		public static void PlayFlybySound( string sound )
		{
			Sound.FromScreen( sound );
		}

		public static float GetDamageFalloff( float distance, float damage, float start, float end )
		{
			if ( end > 0f )
			{
				if ( start > 0f )
				{
					if ( distance < start )
					{
						return damage;
					}

					var falloffRange = end - start;
					var difference = (distance - start);

					return Math.Max( damage - (damage / falloffRange) * difference, 0f );
				}

				return Math.Max( damage - (damage / end) * distance, 0f );
			}

			return damage;
		}

		public static void PlayFlybySounds( Entity attacker, Entity victim, Vector3 start, Vector3 end, float minDistance, float maxDistance, List<string> sounds )
		{
			var sound = Rand.FromList( sounds );

			foreach ( var client in Client.All )
			{
				var pawn = client.Pawn;

				if ( !pawn.IsValid() || pawn == attacker )
					continue;

				if ( pawn.LifeState != LifeState.Alive )
					continue;

				var distance = pawn.Position.DistanceToLine( start, end, out var _ );

				if ( distance >= minDistance && distance <= maxDistance )
				{
					PlayFlybySound( To.Single( client ), sound );
				}
			}
		}

		public static void PlayFlybySounds( Entity attacker, Vector3 start, Vector3 end, float minDistance, float maxDistance, List<string> sounds )
		{
			PlayFlybySounds( attacker, null, start, end, minDistance, maxDistance, sounds );
		}
	}
}
