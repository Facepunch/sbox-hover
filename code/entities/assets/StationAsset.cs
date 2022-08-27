using Gamelib.UI;
using Sandbox;
using System;
using System.Linq;
using SandboxEditor;

namespace Facepunch.Hover
{
	[Library( "hv_station" )]
	[EditorModel( "models/upgrade_station/upgrade_station.vmdl", FixedBounds = true )]
	[Title( "Station" )]
	public partial class StationAsset : GeneratorDependency, IBaseAsset
	{
		public override string IconName => "ui/icons/loadouts.png";

		public Particles IdleParticles { get; private set; }

		private RealTimeUntil NextRestockAvailable { get; set; }
		private WorldStationHud StationHud { get; set; }

		public void ShowUseEffects()
		{
			var particles = Particles.Create( "particles/upgrade_station/upgrade_use.vpcf", this );
			particles.SetPosition( 5, RenderColor * 255f );
		}

		public virtual bool CanPlayerUse( Player player )
		{
			return IsPowered && (Team == Team.None || player.Team == Team);
		}

		public override void Spawn()
		{
			SetModel( "models/upgrade_station/upgrade_station.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			Transmit = TransmitType.Always;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			CreateIdleParticles();

			StationHud = new WorldStationHud();
			StationHud.SetEntity( this, "hud" );

			Hud.UpOffset = 60f;
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
		}

		protected override void OnTeamChanged( Team team )
		{
			CreateIdleParticles();
		}

		protected override void ServerTick()
		{
			base.ServerTick();

			var entities = Entity.FindInSphere( Position, 100f ).OfType<Player>();

			foreach ( var player in entities )
			{
				if ( player.LifeState == LifeState.Alive && CanPlayerUse( player ) )
				{
					if ( NextRestockAvailable && player.TryRestock() )
					{
						NextRestockAvailable = 2f;
						ShowUseEffects();
					}
				}
			}
		}

		protected override void OnIsPoweredChanged( bool isPowered )
		{
			if ( isPowered )
				CreateIdleParticles();
			else
				DestroyIdleParticles();

			if ( isPowered )
				SceneObject.Attributes.Set( "ScrollSpeed", new Vector2( 0f, 1f ) );
			else
				SceneObject.Attributes.Set( "ScrollSpeed", new Vector2( 0f, 0f ) );

			base.OnIsPoweredChanged( isPowered );
		}

		private void CreateIdleParticles()
		{
			IdleParticles?.Destroy();
			IdleParticles = Particles.Create( "particles/upgrade_station/upgrade_idle.vpcf", this );
			IdleParticles.SetPosition( 5, RenderColor * 255f );
		}

		private void DestroyIdleParticles()
		{
			IdleParticles?.Destroy();
			IdleParticles = null;
		}
	}
}

