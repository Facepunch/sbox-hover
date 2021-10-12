using Gamelib.Extensions;
using Sandbox;
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
