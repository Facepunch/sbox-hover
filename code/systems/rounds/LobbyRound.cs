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
			if ( Game.IsServer )
			{
				var players = Game.Clients.Select( ( client ) => client.Pawn as HoverPlayer );

				foreach ( var player in players )
					OnPlayerJoin( player );
			}
		}

		public override void OnPlayerKilled( HoverPlayer player, Entity attacker, DamageInfo damageInfo )
		{
			player.Respawn();

			base.OnPlayerKilled( player, attacker, damageInfo );
		}

		public override void OnPlayerSpawn( HoverPlayer player )
		{
			player.Loadout?.Respawn( player );

			base.OnPlayerSpawn( player );
		}

		public override void OnPlayerJoin( HoverPlayer player )
		{
			if ( Players.Contains( player ) )
			{
				return;
			}

			AddPlayer( player );

			player.Reset();
			player.SetTeam( Game.Random.Float() > 0.5f ? Team.Red : Team.Blue );
			player.GiveLoadout<LightAssault>();
			player.Respawn();

			UI.TutorialScreen.Show( To.Single( player ) );

			base.OnPlayerJoin( player );
		}
	}
}
