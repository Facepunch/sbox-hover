﻿using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		public static bool AllowFriendlyFire { get; set; } = true;

		public Game()
		{
			if ( IsServer ) Hud = new();

			Teams.Initialize();
		}

		public async Task StartSecondTimer()
		{
			while (true)
			{
				await Task.DelaySeconds( 1 );
				OnSecond();
			}
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
				OnKilled( client.Pawn );
			}
		}

		public override void PostLevelLoaded()
		{
			_ = StartSecondTimer();

			base.PostLevelLoaded();
		}

		public override void OnKilled( Entity entity)
		{
			if ( entity is Player player )
				Rounds.Current?.OnPlayerKilled( player );

			base.OnKilled( entity);
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
