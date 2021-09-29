using Gamelib.Extensions;
using Sandbox;
using System.Linq;

namespace Facepunch.Hover
{
    public partial class PlayRound : BaseRound
	{
		public override string RoundName => "PLAY";
		public override int RoundDuration => 0;
		public override bool ShowRoundInfo => true;
		public override bool ShowTimeLeft => true;

		public override void OnPlayerJoin( Player player )
		{
			SpawnPlayer( player );

			base.OnPlayerJoin( player );
		}

		public override void OnPlayerKilled( Player player )
		{
			player.MakeSpectator( player.Position );

			RespawnPlayer( player );

			base.OnPlayerKilled( player );
		}

		public override void OnPlayerSpawn( Player player )
		{
			base.OnPlayerSpawn( player );

			var loadout = player.Loadout;

			if ( loadout != null )
			{
				loadout.Setup();
				loadout.SupplyLoadout();
			}
		}

		protected override void OnStart()
		{
			if ( Host.IsServer )
			{
				var players = Client.All.Select( ( client ) => client.Pawn as Player ).ToList();

				foreach ( var player in players )
				{
					SpawnPlayer( player );
				}
			}
		}

		protected override void OnFinish()
		{
			
		}

		private async void RespawnPlayer( Player player )
		{
			await GameTask.DelaySeconds( 5 );

			player.Respawn();
		}

		private void SpawnPlayer( Player player )
		{
			if ( !Players.Contains( player ) )
				AddPlayer( player );

			player.SetTeam( Team.Red.GetCount() > Team.Blue.GetCount() ? Team.Blue : Team.Red );
			player.GiveLoadout<AssaultLoadout>();
			player.Respawn();
		}
	}
}
