using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Gamelib.Extensions;

namespace Facepunch.Hover
{
	[Library( "hover", Title = "Hover" )]
	partial class Game : Sandbox.Game
	{
		public Hud Hud { get; set; }

		public static Game Instance
		{
			get => Current as Game;
		}

		[ConVar.Server( "hv_min_players", Help = "The minimum players required to start." )]
		public static int MinPlayers { get; set; } = 2;

		[ConVar.Server( "hv_friendly_fire", Help = "Whether or not friendly fire is enabled." )]
		public static bool AllowFriendlyFire { get; set; } = false;

		[ConVar.Server( "hv_starting_tokens", Help = "The amount of tokens players start with." )]
		public static int StartingTokens { get; set; } = 0;

		[ConVar.Server( "hv_toast_duration", Help = "The time that toasts take to disappear." )]
		public static float ToastDuration { get; set; } = 5f;

		[ConVar.Server( "hv_award_duration", Help = "The time that awards take to disappear." )]
		public static float AwardDuration { get; set; } = 3f;

		[ConCmd.Server( "hv_victory" )]
		public static void ShowVictoryScreen( string team )
		{
			if ( team == "blue" )
				VictoryScreen.Show( Team.Blue, 200f );
			else
				VictoryScreen.Show( Team.Red, 200f );
		}

		[ConCmd.Server( "hv_respawn_screen" )]
		public static void DebugRespawnScreen( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				if ( type == "turret" )
				{
					var turret = All.OfType<TurretAsset>().FirstOrDefault();
					RespawnScreen.Show( 30f, turret );
				}
				else if ( type == "suicide" )
				{
					RespawnScreen.Show( 30f, player );
				}
				else if ( type == "deployable" )
				{
					var mine = new JumpMine();
					RespawnScreen.Show( 30f, player, mine );
				}
				else
				{
					RespawnScreen.Show( 30f, player, player.ActiveChild );
				}
			}
		}

		[ConCmd.Server( "hv_killfeed" )]
		public static void DebugKillFeed( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				if ( type == "turret" )
				{
					var turret = All.OfType<TurretAsset>().FirstOrDefault();
					Hud.AddKillFeed( turret, player );
				}
				else if ( type == "suicide" )
				{
					Hud.AddKillFeed( player );
				}
				else if ( type == "deployable" )
				{
					var mine = new JumpMine();
					Hud.AddKillFeed( player, player, mine );
				}
				else
				{
					Hud.AddKillFeed( player, player, player.ActiveChild );
				}
			}
		}

		[ConCmd.Server( "hv_toast" )]
		public static void DebugToast()
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				Hud.ToastAll( $"The blue team have captured Crashed Ship", "ui/icons/blue_outpost.png" );
			}
		}

		[ConCmd.Server( "hv_award" )]
		public static void GiveAward( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				if ( type == "revenge" )
					player.GiveAward<RevengeAward>();
				else if ( type == "capture" )
					player.GiveAward<CaptureFlagAward>();
				else if ( type == "return" )
					player.GiveAward<ReturnFlagAward>();
				else
					player.GiveAward<KillAward>();
			}
		}

		[ConCmd.Server( "hv_switch_teams" )]
		public static void SwitchTeams()
        {
			if ( ConsoleSystem.Caller.Pawn is Player player )
            {
				if ( player.Team == Team.Red )
					player.SetTeam( Team.Blue );
				else
					player.SetTeam( Team.Red );

				player.Respawn();
			}
        }

		public Game()
		{
			if ( IsServer )
			{
				PrecacheAssets();
				Hud = new();
			}

			Awards.Add<KillAward>();
			Awards.Add<AssistAward>();
			Awards.Add<DoubleKillAward>();
			Awards.Add<TripleKillAward>();
			Awards.Add<KillingSpreeAward>();
			Awards.Add<DemolitionManAward>();
			Awards.Add<BuzzkillAward>();
			Awards.Add<CaptureFlagAward>();
			Awards.Add<ReturnFlagAward>();
			Awards.Add<RevengeAward>();
			Awards.Add<FirstBloodAward>();
			Awards.Add<CaptureOutpostAward>();
		}

		public async Task StartSecondTimer()
		{
			while (true)
			{
				await Task.DelaySeconds( 1 );
				OnSecond();
			}
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			if ( pawn is Player player )
			{
				var team = player.Team;

				if ( team == Team.None )
					team = Team.Blue;

				var spawnpoints = All.OfType<PlayerSpawnpoint>()
					.Where( e => e.Team == team )
					.ToList()
					.Shuffle();

				if ( spawnpoints.Count > 0 )
				{
					var spawnpoint = spawnpoints[0];
					player.Transform = spawnpoint.Transform;
					return;
				}
			}

			base.MoveToSpawnpoint( pawn );
		}

		public override bool CanHearPlayerVoice( Client sourceClient, Client destinationClient )
		{
			Host.AssertServer();

			var sourcePlayer = sourceClient.Pawn as Player;
			var destinationPlayer = destinationClient.Pawn as Player;

			if ( sourcePlayer != null && destinationPlayer != null )
			{
				return sourcePlayer.Team == destinationPlayer.Team;
			}

			return false;
		}

		public override void DoPlayerNoclip( Client client )
		{
			// Do nothing. The player can't noclip in this mode.
		}

		public override void DoPlayerSuicide( Client client )
		{
			if ( client.Pawn.LifeState == LifeState.Alive )
			{
				// This simulates the player being killed.
				client.Pawn.LifeState = LifeState.Dead;
				client.Pawn.OnKilled();
			}
		}

		public override void PostLevelLoaded()
		{
			_ = StartSecondTimer();

			base.PostLevelLoaded();
		}

		public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
		{
			Rounds.Current?.OnPlayerLeave( client.Pawn as Player );

			foreach ( var flag in All.OfType<FlagEntity>() )
			{
				if ( flag.Carrier == client.Pawn )
				{
					flag.Drop( true );
				}
			} 

			base.ClientDisconnect( client, reason );
		}

		public override void ClientJoined( Client client )
		{
			var player = new Player();
			client.Pawn = player;

			Rounds.Current?.OnPlayerJoin( player );

			base.ClientJoined( client );
		}

		private void PrecacheAssets()
		{
			var assets = FileSystem.Mounted.ReadJsonOrDefault<List<string>>( "resources/hover.assets.json" );

			foreach ( var asset in assets )
			{
				Log.Info( $"Precaching: {asset}" );
				Precache.Add( asset );
			}
		}

		private void OnSecond()
		{
			CheckMinimumPlayers();
		}

		[Event.Entity.PostSpawn]
		private void OnEntityPostSpawn()
		{
			if ( IsServer )
			{
				Rounds.Change( new LobbyRound() );
			}
		}

		private void CheckMinimumPlayers()
		{
			if ( Client.All.Count >= MinPlayers )
			{
				if ( Rounds.Current is LobbyRound || Rounds.Current == null )
				{
					Rounds.Change( new PlayRound() );
				}
			}
			else if ( Rounds.Current is not LobbyRound )
			{
				Rounds.Change( new LobbyRound() );
			}
		}
	}
}
