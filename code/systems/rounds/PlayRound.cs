using Sandbox;
using System.Linq;

namespace Facepunch.Hover
{
    public partial class PlayRound : BaseRound
	{
		public override string RoundName => "CTF";
		public override int RoundDuration => 1200;
		public override bool ShowRoundInfo => true;
		public override bool ShowTimeLeft => true;

		[Net] public int BlueScore { get; set; }
		[Net] public int RedScore { get; set; }
		
		private RoundScore ScoreHud { get; set; }
		private bool HasFirstBlood { get; set; }

		public int GetScore( Team team )
		{
			return team == Team.Blue ? BlueScore : RedScore;
		}

		public override void OnPlayerJoin( Player player )
		{
			SpawnPlayer( player );

			base.OnPlayerJoin( player );
		}

		public override void OnPlayerKilled( Player player, Player attacker, DamageInfo damageInfo )
		{
			if ( attacker.IsValid() )
			{
				if ( !HasFirstBlood )
				{
					attacker.GiveAward<FirstBloodAward>();
					HasFirstBlood = true;
				}

				if ( player == attacker.LastKiller )
				{
					attacker.GiveAward<RevengeAward>();
					attacker.LastKiller = null;
				}

				attacker.GiveAward<KillAward>();

				if ( player.KillStreak > 2 )
				{
					attacker.GiveAward<BuzzkillAward>();
				}
			}

			player.MakeSpectator( player.Position, 5f );

			base.OnPlayerKilled( player, attacker, damageInfo );
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
				FlagSpawnpoint.OnFlagCaptured += OnFlagCaptured;
				FlagSpawnpoint.OnFlagReturned += OnFlagReturned;

				BlueScore = 0;
				RedScore = 0;

				var players = Client.All.Select( ( client ) => client.Pawn as Player ).ToList();

				foreach ( var player in players )
				{
					SpawnPlayer( player );
				}
			}
			else
			{
				ScoreHud = Local.Hud.AddChild<RoundScore>();
			}
		}

		protected override void OnFinish()
		{
			if ( Host.IsServer )
			{
				FlagSpawnpoint.OnFlagCaptured -= OnFlagCaptured;
				FlagSpawnpoint.OnFlagReturned -= OnFlagReturned;
			}
			else
			{
				ScoreHud?.Delete();
			}
		}

		private void OnFlagReturned( Player player, FlagEntity flag )
		{
			
		}

		private void OnFlagCaptured( Player player, FlagEntity flag )
		{
			if ( player.Team == Team.Blue )
				BlueScore++;
			else
				RedScore++;
		}

		private void SpawnPlayer( Player player )
		{
			if ( !Players.Contains( player ) )
				AddPlayer( player );

			player.Reset();
			player.SetTeam( Team.Red.GetCount() > Team.Blue.GetCount() ? Team.Blue : Team.Red );
			player.GiveLoadout<LightAssault>();
			player.Respawn();
		}
	}
}
