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

		[ServerVar( "hv_min_players", Help = "The minimum players required to start." )]
		public static int MinPlayers { get; set; } = 2;

		[ServerVar( "hv_friendly_fire", Help = "Whether or not friendly fire is enabled." )]
		public static bool AllowFriendlyFire { get; set; } = false;

		public Game()
		{
			if ( IsServer ) Hud = new();

			Awards.Add<KillAward>();
			Awards.Add<AssistAward>();
			Awards.Add<BuzzkillAward>();
			Awards.Add<CaptureFlagAward>();
			Awards.Add<ReturnFlagAward>();
			Awards.Add<RevengeAward>();
			Awards.Add<FirstBloodAward>();
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

			base.ClientDisconnect( client, reason );
		}

		public override void ClientJoined( Client client )
		{
			var player = new Player();
			client.Pawn = player;

			Rounds.Current?.OnPlayerJoin( player );

			base.ClientJoined( client );
		}

		private void OnSecond()
		{
			CheckMinimumPlayers();
		}

		[Event.Tick]
		private void OnTick()
		{
			Rounds.Current?.OnTick();
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
