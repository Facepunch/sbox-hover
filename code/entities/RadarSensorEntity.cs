using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_radar_sensor" )]
	[Hammer.EditorModel( "models/tempmodels/turret/turret.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Radar Sensor", "Hover", "Defines a point where a sensor spawns" )]
	[Hammer.Sphere( 4000, 75, 75, 255 )]
	public partial class RadarSensorEntity : GeneratorDependency
	{
		public float Range { get; set; } = 4000f;

		private RealTimeUntil NextSensePlayers { get; set; }
		private Sound IdleSound { get; set; }

		public override void Spawn()
		{
			SetModel( "models/tempmodels/turret/turret.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			PlayIdleSound();

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else if ( Team == Team.Red )
				RenderColor = Color.Red;

			Name = "Radar Sensor";

			base.Spawn();
		}

		public void StopIdleSound()
		{
			IdleSound.Stop();
		}

		public void PlayIdleSound()
		{
			IdleSound.Stop();
			IdleSound = PlaySound( "radar.idle" );
		}

		public override void OnGameReset()
		{
			base.OnGameReset();

			PlayIdleSound();
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
		}

		protected override void OnGeneratorRepaired( GeneratorEntity generator )
		{
			base.OnGeneratorRepaired( generator );

			PlayIdleSound();
		}

		protected override void OnGeneratorBroken( GeneratorEntity generator )
		{
			base.OnGeneratorBroken( generator );

			StopIdleSound();
		}

		protected override void OnIsPoweredChanged( bool isPowered )
		{
			if ( isPowered )
				SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 1f ) );
			else
				SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 0f ) );

			base.OnIsPoweredChanged( isPowered );
		}

		private bool IsValidTarget( Player player )
		{
			if ( player.LifeState == LifeState.Dead )
				return false;

			if ( player.Team == Team )
				return false;

			if ( player.HasWeapon<RadarJammer>() )
				return false;

			return true;
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( !IsPowered || !NextSensePlayers ) return;

			var players = Physics.GetEntitiesInSphere( Position, Range )
				.OfType<Player>()
				.Where( IsValidTarget );

			var didFindPlayer = false;

			foreach ( var player in players )
			{
				if ( player.HideOnRadarTime )
				{
					player.HideOnRadarTime = 3f;
					didFindPlayer = true;
				}
			}

			if ( didFindPlayer )
			{
				PlaySound( "radar.beep" );
			}

			NextSensePlayers = 2f;
		}
	}
}
