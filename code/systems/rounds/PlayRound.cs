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

			TutorialScreen.Show( To.Single( player ) );

			base.OnPlayerJoin( player );
		}

		public override void OnPlayerKilled( Player player, Entity attacker, DamageInfo damageInfo )
		{
			if ( attacker.IsValid() && attacker is Player killer )
			{
				if ( player.IsEnemyPlayer( killer ) )
				{
					if ( !HasFirstBlood )
					{
						killer.GiveAward<FirstBloodAward>();
						HasFirstBlood = true;
					}

					if ( player == killer.LastKiller )
					{
						killer.GiveAward<RevengeAward>();
						killer.LastKiller = null;
					}

					killer.GiveAward<KillAward>();

					if ( player.KillStreak > 2 )
					{
						killer.GiveAward<BuzzkillAward>();
					}
				}

				if ( player == killer && damageInfo.Flags.HasFlag( DamageFlags.Fall ) )
					Hud.AddKillFeed( To.Everyone, killer, player, null );
				else
					Hud.AddKillFeed( To.Everyone, killer, player, attacker.ActiveChild as Weapon );
			}
			else if ( attacker is IKillFeedIcon )
			{
				Hud.AddKillFeed( To.Everyone, attacker, player );
			}
			else
			{
				Hud.AddKillFeed( To.Everyone, player );
			}

			var assister = player.GetBestAssist( attacker );

			if ( assister.IsValid() )
			{
				assister.GiveAward<AssistAward>();
			}

			RespawnScreen.Show( To.Single( player ), 5f, attacker );

			player.MakeSpectator( player.Position, 5f );

			base.OnPlayerKilled( player, attacker, damageInfo );
		}

		public override void OnPlayerSpawn( Player player )
		{
			base.OnPlayerSpawn( player );

			RespawnScreen.Hide( To.Single( player ) );

			var loadout = player.Loadout;

			if ( loadout != null )
			{
				loadout.Setup( player );
				loadout.SupplyLoadout( player );
			}
		}

		protected override void OnStart()
		{
			if ( Host.IsServer )
			{
				GeneratorEntity.OnGeneratorRepaired += OnGeneratorRepaired;
				GeneratorEntity.OnGeneratorBroken += OnGeneratorBroken;
				FlagSpawnpoint.OnFlagCaptured += OnFlagCaptured;
				FlagSpawnpoint.OnFlagReturned += OnFlagReturned;
				FlagEntity.OnFlagPickedUp += OnFlagPickedUp;
				FlagEntity.OnFlagDropped += OnFlagDropped;

				foreach ( var resettable in Entity.All.OfType<IGameResettable>() )
				{
					resettable.OnGameReset();
				}

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

		protected override void OnTimeUp()
		{
			Finish();

			base.OnTimeUp();
		}

		protected override void OnFinish()
		{
			if ( Host.IsServer )
			{
				if ( BlueScore > RedScore )
				{
					VictoryScreen.Show( Team.Blue, 10f );
					Audio.Play( $"blue.victory{Rand.Int( 1, 2 )}" );
				}
				else if ( RedScore > BlueScore )
				{
					VictoryScreen.Show( Team.Red, 10f );
					Audio.Play( $"red.victory{Rand.Int( 1, 2 )}" );
				}
				else
				{
					VictoryScreen.Show( Team.None, 10f );
				}

				GeneratorEntity.OnGeneratorRepaired -= OnGeneratorRepaired;
				GeneratorEntity.OnGeneratorBroken -= OnGeneratorBroken;
				FlagSpawnpoint.OnFlagCaptured -= OnFlagCaptured;
				FlagSpawnpoint.OnFlagReturned -= OnFlagReturned;
				FlagEntity.OnFlagPickedUp -= OnFlagPickedUp;
				FlagEntity.OnFlagDropped -= OnFlagDropped;

				Rounds.Change( new StatsRound() );
			}
			else
			{
				ScoreHud?.Delete();
			}
		}

		private void OnGeneratorBroken( GeneratorEntity generator )
		{
			Audio.Play( generator.Team, $"generatordestroyed{Rand.Int( 1, 2 )}", $"generatordestroyed{Rand.Int( 1, 2 )}" );
		}


		private void OnGeneratorRepaired( GeneratorEntity generator )
		{
			Audio.Play( generator.Team, $"generatorrepaired{Rand.Int( 1, 2 )}", $"generatorrepaired{Rand.Int( 1, 2 )}" );
		}

		private void OnFlagDropped( Player player, FlagEntity flag )
		{
			Audio.Play( flag.Team, $"flagdropped{Rand.Int( 1, 2 )}", $"flagdropped{Rand.Int( 1, 2 )}" );

			if ( flag.Team == Team.Blue )
				Hud.ToastAll( player.Client.Name + " dropped the Blue flag", "ui/icons/flag-blue.png" );
			else
				Hud.ToastAll( player.Client.Name + " dropped the Red flag", "ui/icons/flag-red.png" );
		}

		private void OnFlagPickedUp( Player player, FlagEntity flag )
		{
			Audio.Play( flag.Team, $"flagtaken{Rand.Int( 1, 2 )}", $"flagtaken{Rand.Int( 1, 2 )}" );

			if ( flag.Team == Team.Blue )
				Hud.ToastAll( player.Client.Name + " picked up the Blue flag", "ui/icons/flag-blue.png" );
			else
				Hud.ToastAll( player.Client.Name + " picked up the Red flag", "ui/icons/flag-red.png" );
		}

		private void OnFlagReturned( Player player, FlagEntity flag )
		{
			Audio.Play( flag.Team, $"flagreturned{Rand.Int( 1, 2 )}", $"flagreturned{Rand.Int( 1, 2 )}" );

			if ( flag.Team == Team.Blue )
				Hud.ToastAll( player.Client.Name + " returned the Blue flag", "ui/icons/flag-blue.png" );
			else
				Hud.ToastAll( player.Client.Name + " returned the Red flag", "ui/icons/flag-red.png" );
		}

		private void OnFlagCaptured( Player player, FlagEntity flag )
		{
			Audio.Play( flag.Team, $"flagcaptured{Rand.Int( 1, 2 )}", $"flagcaptured{Rand.Int( 1, 2 )}" );

			if ( flag.Team == Team.Blue )
				Hud.ToastAll( player.Client.Name + " captured the Blue flag", "ui/icons/flag-blue.png" );
			else
				Hud.ToastAll( player.Client.Name + " captured the Red flag", "ui/icons/flag-red.png" );

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
