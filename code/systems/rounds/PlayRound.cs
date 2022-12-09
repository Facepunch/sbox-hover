﻿using Gamelib.Extensions;
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

				if ( player == killer && damageInfo.Flags.HasFlag( DamageFlags.Fall ) )
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
			if ( Host.IsServer )
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

				var players = Client.All.Select( ( client ) => client.Pawn as HoverPlayer ).ToList();

				foreach ( var player in players )
				{
					SpawnPlayer( player );
				}
			}
			else
			{
				ScoreHud = Local.Hud.AddChild<UI.RoundScore>();
			}
		}
		

		protected override void OnTimeUp()
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

				UI.StationScreen.Hide();

				GeneratorAsset.OnGeneratorRepaired -= OnGeneratorRepaired;
				GeneratorAsset.OnGeneratorBroken -= OnGeneratorBroken;
				OutpostVolume.OnOutpostCaptured -= OnOutpostCaptured;
				OutpostVolume.OnOutpostLost -= OnOutpostLost;
				FlagSpawnpoint.OnFlagCaptured -= OnFlagCaptured;
				FlagEntity.OnFlagReturned -= OnFlagReturned;
				FlagEntity.OnFlagPickedUp -= OnFlagPickedUp;
				FlagEntity.OnFlagDropped -= OnFlagDropped;

				Game.ChangeRound( new StatsRound() );
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

		[Event( "refresh" )]
		private void OnRefresh()
		{
			ScoreHud = Local.Hud.AddChild<UI.RoundScore>();
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
			Audio.Play( generator.Team, $"your.generatordestroyed{Rand.Int( 1, 2 )}", $"generatordestroyed{Rand.Int( 1, 2 )}" );

			var attacker = generator.LastAttacker as HoverPlayer;

			if ( attacker.IsValid() )
			{
				attacker.GiveAward<DemolitionManAward>();
			}
		}

		private void OnGeneratorRepaired( GeneratorAsset generator )
		{
			Audio.Play( generator.Team, $"your.generatorrepaired{Rand.Int( 1, 2 )}", $"generatorrepaired{Rand.Int( 1, 2 )}" );
		}

		private void OnFlagDropped( HoverPlayer player, FlagEntity flag )
		{
			if ( NextFlagAnnouncement )
			{
				Audio.Play( flag.Team, $"your.flagdropped{Rand.Int( 1, 2 )}", $"flagdropped{Rand.Int( 1, 2 )}" );
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
				Audio.Play( flag.Team, $"your.flagtaken{Rand.Int( 1, 2 )}", $"flagtaken{Rand.Int( 1, 2 )}" );
				NextFlagAnnouncement = 5f;
			}

			if ( flag.Team == Team.Blue )
				UI.Hud.ToastAll( player.Client.Name + " picked up the Blue flag", "ui/icons/flag-blue.png" );
			else
				UI.Hud.ToastAll( player.Client.Name + " picked up the Red flag", "ui/icons/flag-red.png" );
		}

		private void OnFlagReturned( HoverPlayer player, FlagEntity flag )
		{
			Audio.Play( flag.Team, $"your.flagreturned{Rand.Int( 1, 2 )}", $"flagreturned{Rand.Int( 1, 2 )}" );

			if ( flag.Team == Team.Blue )
				UI.Hud.ToastAll( player.Client.Name + " returned the Blue flag", "ui/icons/flag-blue.png" );
			else
				UI.Hud.ToastAll( player.Client.Name + " returned the Red flag", "ui/icons/flag-red.png" );
		}

		private void OnFlagCaptured( HoverPlayer player, FlagEntity flag )
		{
			Audio.Play( flag.Team, $"your.flagcaptured{Rand.Int( 1, 2 )}", $"flagcaptured{Rand.Int( 1, 2 )}" );

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
				Game.Instance.MoveToSpawnpoint( player );
				player.MakeSpectator( player.Position );
			}
			else
			{
				player.Respawn();
			}
		}
	}
}
