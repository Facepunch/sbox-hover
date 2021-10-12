using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_station" )]
	[Hammer.EditorModel( "models/upgrade_station/upgrade_station.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Station", "Hover", "Defines a point where a team's station spawns" )]
	public partial class StationEntity : GeneratorDependency
	{
		public Particles IdleParticles { get; private set; }

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
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else if ( Team == Team.Red )
				RenderColor = Color.Red;
			else if ( Team == Team.None )
				RenderColor = Color.Yellow;

			Name = "Station";

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			CreateIdleParticles();

			StationHud = new WorldStationHud();
			StationHud.SetEntity( this, "hud" );

			Hud.UpOffset = 0f;
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
		}

		protected override void OnIsPoweredChanged( bool isPowered )
		{
			if ( isPowered )
				CreateIdleParticles();
			else
				DestroyIdleParticles();

			if ( isPowered )
				SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 1f ) );
			else
				SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 0f ) );

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

		[Event.Tick.Server]
		private void ServerTick()
		{
			var entities = Physics.GetEntitiesInSphere( Position, 100f ).OfType<Player>();

			foreach ( var player in entities )
			{
				if ( player.LifeState == LifeState.Alive && CanPlayerUse( player ) )
				{
					ShowUseEffects();
					player.TryRestock();
				}
			}
		}
	}
}
