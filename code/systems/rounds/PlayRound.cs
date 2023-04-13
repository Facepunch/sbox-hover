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
		public override bool CanCaptureFlags => true;
		public override bool CanCaptureOutposts => true;

		[Net] public int BlueScore { get; set; }
		[Net] public int RedScore { get; set; }
		
		private RealTimeUntil NextFlagAnnouncement { get; set; }
		private UI.RoundScore ScoreHud { get; set; }
		private bool HasFirstBlood { get; set; }

		public int GetScore( Team team )
		{
			return team == Team.Blue ? BlueScore : RedScore;
		}

		public override void OnPlayerJoin( HoverPlayer player )
		{
			SpawnPlayer( player );

			UI.TutorialScreen.Show( To.Single( player ) );

			base.OnPlayerJoin( player );
		}

		public override void OnPlayerKilled( HoverPlayer player, Entity attacker, DamageInfo damageInfo )
		{
			var assister = player.GetBestAssist( attacker );

			if ( assister.IsValid() )
			{
				if ( player != attacker )
					assister.GiveAward<AssistAward>();
				else
					attacker = assister;
			}

			if ( attacker.IsValid() && attacker is HoverPlayer killer )
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
						killer.GiveAward<BuzzkillAward>();

					if ( killer.KillStreak == 4 )
						killer.GiveAward<KillingSpreeAward>();

					if ( killer.SuccessiveKills == 1 )
						killer.GiveAward<DoubleKillAward>();
					else if ( killer.SuccessiveKills == 2 )
						killer.GiveAward<TripleKillAward>();
				}

				if ( player == killer && damageInfo.HasTag( "fall" ) )
					UI.Hud.AddKillFeed( To.Everyone, killer, player, null );
				else if ( damageInfo.Weapon.IsValid() )
					UI.Hud.AddKillFeed( To.Everyone, killer, player, damageInfo.Weapon );
				else
					UI.Hud.AddKillFeed( To.Everyone, killer, player, killer.ActiveChild as Weapon );
			}
			else if ( attacker is IKillFeedIcon )
			{
				UI.Hud.AddKillFeed( To.Everyone, attacker, player );
			}
			else
			{
				UI.Hud.AddKillFeed( To.Everyone, player );
			}

			UI.RespawnScreen.Show( To.Single( player ), 5f, attacker );

			player.MakeSpectator( player.Position, 5f );

			base.OnPlayerKilled( player, attacker, damageInfo );
		}

		public override void OnPlayerSpawn( HoverPlayer player )
		{
			base.OnPlayerSpawn( player );

			UI.RespawnScreen.Hide( To.Single( player ) );

			var loadout = player.Loadout;

			if ( loadout != null )
			{
				loadout.Respawn( player );
				loadout.Supply( player );
			}
		}

		protected override void OnStart()
		{
			if ( Game.IsServer )
			{
				GeneratorAsset.OnGeneratorRepaired += OnGeneratorRepaired;
				GeneratorAsset.OnGeneratorBroken += OnGeneratorBroken;
				OutpostVolume.OnOutpostCaptured += OnOutpostCaptured;
				OutpostVolume.OnOutpostLost += OnOutpostLost;
				FlagSpawnpoint.OnFlagCaptured += OnFlagCaptured;
				FlagEntity.OnFlagReturned += OnFlagReturned;
				FlagEntity.OnFlagPickedUp += OnFlagPickedUp;
				FlagEntity.OnFlagDropped += OnFlagDropped;

				var resettables = Entity.All.OfType<IGameResettable>().ToList();

				foreach ( var resettable in resettables )
				{
					resettable.OnGameReset();
				}

				BlueScore = 0;
				RedScore = 0;

				var players = Game.Clients.Select( ( client ) => client.Pawn as HoverPlayer ).ToList();

				foreach ( var player in players )
				{
					SpawnPlayer( player );
				}
			}
			else
			{
				ScoreHud = Game.RootPanel.AddChild<UI.RoundScore>();
			}
		}
		

		protected override void OnTimeUp()
		{
			if ( Game.IsServer )
			{
				if ( BlueScore > RedScore )
				{
					VictoryScreen.Show( Team.Blue, 10f );
					Audio.Play( $"blue.victory{Game.Random.Int( 1, 2 )}" );
				}
				else if ( RedScore > BlueScore )
				{
					VictoryScreen.Show( Team.Red, 10f );
					Audio.Play( $"red.victory{Game.Random.Int( 1, 2 )}" );
				}
				else
				{
					VictoryScreen.Show( Team.None, 10f );
				}

				UI.StationScreen.Hide();

				GeneratorAsset.OnGeneratorRepaired -= OnGeneratorRepaired;
				GeneratorAsset.OnGeneratorBroken -= OnGeneratorBroken;
				OutpostVolume.OnOutpostCaptured -= OnOutpostCaptured;
				OutpostVolume.OnOutpostLost -= OnOutpostLost;
				FlagSpawnpoint.OnFlagCaptured -= OnFlagCaptured;
				FlagEntity.OnFlagReturned -= OnFlagReturned;
				FlagEntity.OnFlagPickedUp -= OnFlagPickedUp;
				FlagEntity.OnFlagDropped -= OnFlagDropped;

				HoverGame.ChangeRound( new StatsRound() );
			}
			else
			{
				ScoreHud?.Delete();
			}

			base.OnTimeUp();
		}

		protected override void OnFinish()
		{

		}

		private void OnOutpostLost( OutpostVolume outpost )
		{
			Audio.Play( outpost.LastCapturer, "you.lostoutpost", "lostoutpost" );

			if ( outpost.LastCapturer == Team.Blue )
				UI.Hud.ToastAll( $"The blue team have lost {outpost.OutpostName}", "ui/icons/neutral_outpost.png" );
			else
				UI.Hud.ToastAll( $"The red team have lost {outpost.OutpostName}", "ui/icons/neutral_outpost.png" );
		}

		private void OnOutpostCaptured( OutpostVolume outpost )
		{
			foreach ( var player in outpost.TouchingEntities.OfType<HoverPlayer>() )
			{
				if ( player.LifeState == LifeState.Alive && player.Team == outpost.Team )
				{
					player.GiveAward<CaptureOutpostAward>();
				}
			}

			Audio.Play( outpost.Team, "you.takenoutpost", "takenoutpost" );

			if ( outpost.Team == Team.Blue )
				UI.Hud.ToastAll( $"The blue team have captured {outpost.OutpostName}", "ui/icons/blue_outpost.png" );
			else
				UI.Hud.ToastAll( $"The red team have captured {outpost.OutpostName}", "ui/icons/red_outpost.png" );
		}

		private void OnGeneratorBroken( GeneratorAsset generator )
		{
			Audio.Play( generator.Team, $"your.generatordestroyed{Game.Random.Int( 1, 2 )}", $"generatordestroyed{Game.Random.Int( 1, 2 )}" );

			var attacker = generator.LastAttacker as HoverPlayer;

			if ( attacker.IsValid() )
			{
				attacker.GiveAward<DemolitionManAward>();
			}
		}

		private void OnGeneratorRepaired( GeneratorAsset generator )
		{
			Audio.Play( generator.Team, $"your.generatorrepaired{Game.Random.Int( 1, 2 )}", $"generatorrepaired{Game.Random.Int( 1, 2 )}" );
		}

		private void OnFlagDropped( HoverPlayer player, FlagEntity flag )
		{
			if ( NextFlagAnnouncement )
			{
				Audio.Play( flag.Team, $"your.flagdropped{Game.Random.Int( 1, 2 )}", $"flagdropped{Game.Random.Int( 1, 2 )}" );
				NextFlagAnnouncement = 5f;
			}

			if ( flag.Team == Team.Blue )
				UI.Hud.ToastAll( player.Client.Name + " dropped the Blue flag", "ui/icons/flag-blue.png" );
			else
				UI.Hud.ToastAll( player.Client.Name + " dropped the Red flag", "ui/icons/flag-red.png" );
		}

		private void OnFlagPickedUp( HoverPlayer player, FlagEntity flag )
		{
			if ( NextFlagAnnouncement )
			{
				Audio.Play( flag.Team, $"your.flagtaken{Game.Random.Int( 1, 2 )}", $"flagtaken{Game.Random.Int( 1, 2 )}" );
				NextFlagAnnouncement = 5f;
			}

			if ( flag.Team == Team.Blue )
				UI.Hud.ToastAll( player.Client.Name + " picked up the Blue flag", "ui/icons/flag-blue.png" );
			else
				UI.Hud.ToastAll( player.Client.Name + " picked up the Red flag", "ui/icons/flag-red.png" );
		}

		private void OnFlagReturned( HoverPlayer player, FlagEntity flag )
		{
			Audio.Play( flag.Team, $"your.flagreturned{Game.Random.Int( 1, 2 )}", $"flagreturned{Game.Random.Int( 1, 2 )}" );

			if ( flag.Team == Team.Blue )
				UI.Hud.ToastAll( player.Client.Name + " returned the Blue flag", "ui/icons/flag-blue.png" );
			else
				UI.Hud.ToastAll( player.Client.Name + " returned the Red flag", "ui/icons/flag-red.png" );
		}

		private void OnFlagCaptured( HoverPlayer player, FlagEntity flag )
		{
			Audio.Play( flag.Team, $"your.flagcaptured{Game.Random.Int( 1, 2 )}", $"flagcaptured{Game.Random.Int( 1, 2 )}" );

			if ( flag.Team == Team.Blue )
				UI.Hud.ToastAll( player.Client.Name + " captured the Blue flag", "ui/icons/flag-blue.png" );
			else
				UI.Hud.ToastAll( player.Client.Name + " captured the Red flag", "ui/icons/flag-red.png" );

			if ( player.Team == Team.Blue )
				BlueScore++;
			else
				RedScore++;
		}

		private void SpawnPlayer( HoverPlayer player )
		{
			if ( !Players.Contains( player ) )
				AddPlayer( player );

			player.Reset();
			player.SetTeam( Team.Red.GetCount() > Team.Blue.GetCount() ? Team.Blue : Team.Red );

			// Keep any previously selected loadout.
			if ( player.Loadout == null )
				player.GiveLoadout<LightAssault>();

			if ( player.Client.IsBot == false )
			{
				UI.StationScreen.Show( To.Single( player ), UI.StationScreenMode.Deployment );
				HoverGame.Entity.MoveToSpawnpoint( player );
				player.MakeSpectator( player.Position );
			}
			else
			{
				player.Respawn();
			}
		}
	}
}
