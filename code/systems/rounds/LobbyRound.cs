using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Hover
{
    public class LobbyRound : BaseRound
	{
		public override string RoundName => "LOBBY";
		public override bool ShowRoundInfo => true;

		protected override void OnStart()
		{
			if ( Host.IsServer )
			{
				var players = Client.All.Select( ( client ) => client.Pawn as Player );

				foreach ( var player in players )
					OnPlayerJoin( player );
			}
		}

		public override void OnPlayerKilled( Player player, Entity attacker, DamageInfo damageInfo )
		{
			player.Respawn();

			base.OnPlayerKilled( player, attacker, damageInfo );
		}

		public override void OnPlayerSpawn( Player player )
		{
			player.Loadout?.Setup();

			base.OnPlayerSpawn( player );
		}

		public override void OnPlayerJoin( Player player )
		{
			if ( Players.Contains( player ) )
			{
				return;
			}

			AddPlayer( player );

			player.Reset();
			player.SetTeam( Rand.Float() > 0.5f ? Team.Red : Team.Blue );
			player.GiveLoadout<LightAssault>();
			player.Respawn();

			base.OnPlayerJoin( player );
		}
	}
}
