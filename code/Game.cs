using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Gamelib.Extensions;

namespace Facepunch.Hover
{
	partial class Game : GameManager
	{
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

		public static void ChangeRound( BaseRound round )
		{
			Assert.NotNull( round );

			Instance.InternalRound?.Finish();
			Instance.InternalRound = round;
			Instance.InternalRound?.Start();
		}

		[ConCmd.Server( "hv_respawn_screen" )]
		public static void DebugRespawnScreen( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
			{
				if ( type == "turret" )
				{
					var turret = All.OfType<TurretAsset>().FirstOrDefault();
					UI.RespawnScreen.Show( 30f, turret );
				}
				else if ( type == "suicide" )
				{
					UI.RespawnScreen.Show( 30f, player );
				}
				else if ( type == "deployable" )
				{
					var mine = new JumpMine();
					UI.RespawnScreen.Show( 30f, player, mine );
				}
				else
				{
					UI.RespawnScreen.Show( 30f, player, player.ActiveChild );
				}
			}
		}

		[ConCmd.Server( "hv_killfeed" )]
		public static void DebugKillFeed( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
			{
				if ( type == "turret" )
				{
					var turret = All.OfType<TurretAsset>().FirstOrDefault();
					UI.Hud.AddKillFeed( turret, player );
				}
				else if ( type == "suicide" )
				{
					UI.Hud.AddKillFeed( player );
				}
				else if ( type == "deployable" )
				{
					var mine = new JumpMine();
					UI.Hud.AddKillFeed( player, player, mine );
				}
				else
				{
					UI.Hud.AddKillFeed( player, player, player.ActiveChild );
				}
			}
		}

		[ConCmd.Server( "hv_toast" )]
		public static void DebugToast()
		{
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
			{
				UI.Hud.ToastAll( $"The blue team have captured Crashed Ship", "ui/icons/blue_outpost.png" );
			}
		}

		[ConCmd.Server( "hv_award" )]
		public static void GiveAward( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
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
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
            {
				if ( player.Team == Team.Red )
					player.SetTeam( Team.Blue );
				else
					player.SetTeam( Team.Red );

				player.Respawn();
			}
        }

		public static BaseRound Round => Instance?.InternalRound;

		[Net, Change( nameof( OnRoundChanged ) )] private BaseRound InternalRound { get; private set; }

		private TimeUntil NextSecondTime { get; set; }

		public override void Spawn()
		{
			PrecacheAssets();
			AddAwards();

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			AddAwards();

			Local.Hud?.Delete( true );
			Local.Hud = new UI.Hud();

			base.ClientSpawn();
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			if ( pawn is HoverPlayer player )
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

			var sourcePlayer = sourceClient.Pawn as HoverPlayer;
			var destinationPlayer = destinationClient.Pawn as HoverPlayer;

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

		public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
		{
			Round?.OnPlayerLeave( client.Pawn as HoverPlayer );

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
			var player = new HoverPlayer();
			client.Pawn = player;

			Round?.OnPlayerJoin( player );

			base.ClientJoined( client );
		}

		private void OnRoundChanged( BaseRound oldRound, BaseRound newRound )
		{
			oldRound?.Finish();
			newRound?.Start();
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

		private void AddAwards()
		{
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

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( NextSecondTime )
			{
				CheckMinimumPlayers();
				Round?.OnSecond();
				NextSecondTime = 1f;
			}
		}

		[Event.Entity.PostSpawn]
		private void OnEntityPostSpawn()
		{
			if ( IsServer )
			{
				ChangeRound( new LobbyRound() );
			}
		}

		private void CheckMinimumPlayers()
		{
			if ( Client.All.Count >= MinPlayers )
			{
				if ( Round is LobbyRound || Round == null )
				{
					ChangeRound( new PlayRound() );
				}
			}
			else if ( Round is not LobbyRound )
			{
				ChangeRound( new LobbyRound() );
			}
		}
	}
}
