using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Editor;

namespace Facepunch.Hover
{
	[Library( "hv_outpost_volume" )]
	[AutoApplyMaterial("materials/editor/hv_outpost_volume.vmat")]
	[Solid]
	[HammerEntity]
	public partial class OutpostVolume : BaseTrigger, IGameResettable, IHudEntity
	{
		public delegate void OutpostEvent( OutpostVolume generator );
		public static event OutpostEvent OnOutpostCaptured;
		public static event OutpostEvent OnOutpostLost;

		public static List<string> Letters { get; private set; } = new()
		{
			"A",
			"B",
			"C",
			"D"
		};

		[Net, Property] public string OutpostName { get; set; }

		[Net] public bool IsBeingCaptured { get; set; }
		[Net] public float CaptureProgress { get; set; }
		[Net] public string Letter { get; set; }
		[Net] public bool IsCaptured { get; set; }
		[Net] public Team CapturingTeam { get; set; }
		[Net] public Team LastCapturer { get; set; }
		[Net] public Team Team { get; set; }

		public UI.EntityHudAnchor Hud { get; private set; }

		public RealTimeUntil NextGiveTokens { get; set; }
		public Vector3 LocalCenter => CollisionBounds.Center;
		public int TokenInterval => 2;
		public int TokenAmount => 1;

		private TimeSince LastPlayCaptureSound { get; set; }
		private bool IsPlayingCaptureSound { get; set; }
		private Sound CaptureSound { get; set; }

		public virtual void OnGameReset()
		{
			IsBeingCaptured = false;
			CaptureProgress = 0f;
			IsCaptured = false;
			CapturingTeam = Team.None;
			LastCapturer = Team.None;
			Team = Team.None;
		}

		public bool ShouldUpdateHud()
		{
			return true;
		}

		public override void Spawn()
		{
			base.Spawn();

			EnableTouchPersists = true;
			EnableDrawing = false;
			Transmit = TransmitType.Always;

			if ( Letters.Count > 0 )
			{
				Letter = Letters[0];
				Letters.RemoveAt( 0 );
			}

			if ( string.IsNullOrEmpty( OutpostName ) )
				OutpostName = $"Outpost {Letter}";
		}

		public override void ClientSpawn()
		{
			Hud = UI.EntityHud.Create( this );

			if ( !string.IsNullOrEmpty( Letter ) )
			{
				var captureHud = Hud.AddChild<UI.OutpostHud>( "outpost" );
				captureHud.SetOutpost( this );
				UI.OutpostList.AddOutpost( this );
			}

			base.ClientSpawn();
		}

		public void UpdateHudComponents()
		{
			if ( Game.LocalPawn is HoverPlayer player )
			{
				var boundsSize = CollisionBounds.Size.Length;
				var distance = player.Position.Distance( Position );
				Hud.Style.Opacity = UIUtil.GetMinMaxDistanceAlpha( distance, boundsSize, 0f, boundsSize + 5000f, boundsSize + 7000f );
			}
		}

		protected void UpdateBaseAssets()
		{
			foreach ( var dependency in Entity.FindInBox( WorldSpaceBounds ) )
			{
				if ( dependency is IBaseAsset asset )
				{
					asset.SetTeam( Team );
				}
			}
		}

		protected void AddProgress( float progress )
		{
			CaptureProgress += progress;

			if ( CaptureProgress > 1f )
				CaptureProgress = 1f;
			else if ( CaptureProgress < 0f )
				CaptureProgress = 0f;
		}

		public override void Touch( Entity other )
		{
			if ( Game.IsClient && other is HoverPlayer player && player.IsLocalPawn )
			{
				if ( IsBeingCaptured && CapturingTeam == player.Team && ( LastCapturer != player.Team || Team != player.Team ) )
				{
					if ( !IsPlayingCaptureSound )
					{
						IsPlayingCaptureSound = true;
						CaptureSound.Stop();
						CaptureSound = Sound.FromScreen( "outpost.captureloop" );
					}

					LastPlayCaptureSound = 0f;
				}
			}
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( IsPlayingCaptureSound && LastPlayCaptureSound >= 0.5f )
			{
				IsPlayingCaptureSound = false;
				CaptureSound.Stop();
			}
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !HoverGame.Round.CanCaptureOutposts ) return;

			var distinctTeams = new HashSet<Team>();

			foreach ( var player in TouchingEntities.OfType<HoverPlayer>() )
			{
				if ( player.LifeState == LifeState.Dead )
					continue;

				distinctTeams.Add( player.Team );
			}

			if ( distinctTeams.Count == 1 )
			{
				IsBeingCaptured = true;
				CapturingTeam = distinctTeams.First();
			}
			else
			{
				IsBeingCaptured = false;
			}

			if ( NextGiveTokens && Team != Team.None && IsCaptured )
			{
				foreach ( var player in Team.GetAll() )
				{
					player.GiveTokens( TokenAmount );
				}

				NextGiveTokens = TokenInterval;
			}

			if ( CapturingTeam == Team && IsCaptured )
				return;

			if ( IsBeingCaptured )
			{
				if ( Team != CapturingTeam )
				{
					AddProgress( -0.1f * Time.Delta );

					if ( CaptureProgress == 0f )
					{
						IsCaptured = false;
						Team = CapturingTeam;
					}
				}
				else
				{
					AddProgress( 0.1f * Time.Delta );

					if ( CaptureProgress == 1f )
					{
						IsBeingCaptured = false;
						LastCapturer = CapturingTeam;
						IsCaptured = true;
						OnOutpostCaptured?.Invoke( this );
						UpdateBaseAssets();
					}
				}
			}
			else
			{
				if ( IsCaptured )
				{
					AddProgress( 0.1f * Time.Delta );
				}
				else
				{
					AddProgress( -0.1f * Time.Delta );

					if ( Team != Team.None && CaptureProgress == 0f )
					{
						if ( LastCapturer != Team.None )
							OnOutpostLost?.Invoke( this );

						LastCapturer = Team.None;
						Team = Team.None;

						UpdateBaseAssets();
					}
				}
			}
		}
	}
}
