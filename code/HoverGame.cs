using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Facepunch.Hover.UI;
using Sandbox.Effects;
using Sandbox.Diagnostics;
using Sandbox.UI;

namespace Facepunch.Hover
{
	partial class HoverGame : GameManager
	{
		public static HoverGame Entity => Current as HoverGame;

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

			Entity.InternalRound?.Finish();
			Entity.InternalRound = round;
			Entity.InternalRound?.Start();
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

		public static BaseRound Round => Entity?.InternalRound;
		private static bool HasInitialized { get; set; }

		[Net, Change( nameof( OnRoundChanged ) )] private BaseRound InternalRound { get; set; }

		private TimeUntil NextSecondTime { get; set; }
		private ScreenEffects PostProcessing { get; set; }

		public override void Spawn()
		{
			PrecacheAssets();
			AddAwards();

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			AddAwards();

			if ( !HasInitialized )
			{
				Game.RootPanel?.Delete( true );

				var hud = new Hud { Style = { ZIndex = 1 } };
				Game.RootPanel = hud;
				
				var anchors = new RootPanel { Style = { ZIndex = -1 } };
				Hud.Anchors = anchors;
				Hud.AddPendingAnchors();
				
				HasInitialized = true;
			}

			PostProcessing = new();
			Camera.Main.RemoveAllHooks();
			Camera.Main.AddHook( PostProcessing );

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

		public override bool CanHearPlayerVoice( IClient src, IClient dest )
		{
			Game.AssertServer();

			var a = src.Pawn as HoverPlayer;
			var b = dest.Pawn as HoverPlayer;

			if ( a != null && b != null )
			{
				return a.Team == b.Team;
			}

			return false;
		}

		public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
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

		public override void ClientJoined( IClient client )
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

		[GameEvent.Tick.Server]
		private void ServerTick()
		{
			if ( NextSecondTime )
			{
				CheckMinimumPlayers();
				Round?.OnSecond();
				NextSecondTime = 1f;
			}
		}

		private float VignetteIntensity { get; set; } = 0f;
		private Color VignetteColor { get; set; } = Color.Black;
		private float Saturation { get; set; }
		private float Pixelation { get; set; }

		[GameEvent.Client.Frame]
		private void OnFrame()
		{
			if ( Game.LocalPawn is not HoverPlayer player )
				return;

			var pp = PostProcessing;
			if ( pp is null ) return;

			pp.ChromaticAberration.Scale = 0.1f;
			pp.ChromaticAberration.Offset = Vector3.Zero;
			pp.Sharpen = 0.1f;

			var healthScale = (0.4f / player.MaxHealth) * player.Health;
			Saturation = 0.7f + healthScale;

			VignetteIntensity = 0.8f - healthScale * 2f;
			VignetteColor = Color.Lerp( Color.Red, Color.Black, (1f / player.MaxHealth) * player.Health).WithAlpha( 0.1f );
			
			pp.Vignette.Smoothness = 1f;
			pp.Vignette.Roundness = 0.8f;

			var sum = ScreenShake.List.OfType<ScreenShake.Random>().Sum( s => (1f - s.Progress) );

			pp.ChromaticAberration.Scale += (0.05f * sum);
			Pixelation = 0.02f * sum;

			if ( player.LifeState == LifeState.Alive )
			{
				var stealthFraction = player.TargetAlpha.Remap( 0f, 1f, 1f, 0f );
				if ( stealthFraction > 0f )
				{
					Pixelation += 0.01f + MathF.Abs( MathF.Sin( Time.Now * 0.1f ) ) * 0.05f * stealthFraction;
					VignetteIntensity += 0.2f * stealthFraction;
					VignetteColor = Color.Lerp( pp.Vignette.Color, Color.White.WithAlpha( 0.1f ), stealthFraction * 0.75f );
					Saturation -= 0.2f * stealthFraction;
				}
			}

			VignetteIntensity = VignetteIntensity.Clamp( 0f, 0.8f );

			pp.Pixelation = pp.Pixelation.LerpTo( Pixelation, Time.Delta * 2f );
			pp.Vignette.Color = Color.Lerp( pp.Vignette.Color, VignetteColor, Time.Delta * 4f );
			pp.Vignette.Intensity = pp.Vignette.Intensity.LerpTo( VignetteIntensity, Time.Delta * 4f );
			pp.Saturation = pp.Saturation.LerpTo( Saturation, Time.Delta * 4f );
		}

		[GameEvent.Entity.PostSpawn]
		private void OnEntityPostSpawn()
		{
			if ( Game.IsServer )
			{
				ChangeRound( new LobbyRound() );
			}
		}

		private void CheckMinimumPlayers()
		{
			if ( Game.Clients.Count >= MinPlayers )
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
