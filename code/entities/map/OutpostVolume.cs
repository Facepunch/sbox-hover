using Gamelib.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_outpost_volume" )]
	[Hammer.AutoApplyMaterial( "materials/editor/hv_jetpack_elevator.vmat" )]
	[Hammer.Solid]
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
		[Net] public Team Team { get; set; }

		public EntityHudAnchor Hud { get; private set; }

		public RealTimeUntil NextGiveTokens { get; set; }
		public Vector3 LocalCenter => CollisionBounds.Center;
		public int TokenInterval => 2;
		public int TokenAmount => 1;

		public virtual void OnGameReset()
		{
			IsBeingCaptured = false;
			CaptureProgress = 0f;
			IsCaptured = false;
			CapturingTeam = Team.None;
			Team = Team.None;
		}

		public bool ShouldUpdateHud()
		{
			return true;
		}

		public override void Spawn()
		{
			base.Spawn();

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
			Hud = EntityHud.Instance.Create( this );

			if ( !string.IsNullOrEmpty( Letter ) )
			{
				var captureHud = Hud.AddChild<OutpostHud>( "outpost" );
				captureHud.SetOutpost( this );
				OutpostList.AddOutpost( this );
			}

			base.ClientSpawn();
		}

		public void UpdateHudComponents()
		{
			if ( Local.Pawn is Player player )
			{
				var boundsSize = CollisionBounds.Size.Length;
				var distance = player.Position.Distance( Position );
				Hud.Style.Opacity = UIUtility.GetMinMaxDistanceAlpha( distance, boundsSize, 0f, boundsSize + 2000f, boundsSize + 5000f );
			}
		}

		protected void UpdateBaseAssets()
		{
			foreach ( var dependency in Physics.GetEntitiesInBox( WorldSpaceBounds ) )
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

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			var distinctTeams = new HashSet<Team>();

			foreach ( var player in TouchingEntities.OfType<Player>() )
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
						OnOutpostLost?.Invoke( this );
						Team = Team.None;
						UpdateBaseAssets();
					}
				}
			}
		}
	}
}
