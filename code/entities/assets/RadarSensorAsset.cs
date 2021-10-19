using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_radar_sensor" )]
	[Hammer.EditorModel( "models/radar_sensor/radar_sensor.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Radar Sensor", "Hover", "Defines a point where a sensor spawns" )]
	[Hammer.Sphere( 4000, 75, 75, 255 )]
	public partial class RadarSensorAsset : GeneratorDependency, IBaseAsset
	{
		public override List<DependencyUpgrade> Upgrades => new()
		{
			new RadarRangeUpgrade(),
			new RadarRangeUpgrade(),
			new RadarRangeUpgrade()
		};

		public float Range { get; set; }

		private RealTimeUntil NextSensePlayers { get; set; }
		private Sound IdleSound { get; set; }

		public override void SetTeam( Team team )
		{
			base.SetTeam( team );

			if ( !IsPowered || team == Team.None )
				StopIdleSound();
			else
				PlayIdleSound();
		}

		public override void Spawn()
		{
			SetModel( "models/radar_sensor/radar_sensor.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			PlayIdleSound();

			Transmit = TransmitType.Always;

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

			if ( Team != Team.None )
			{
				PlayIdleSound();
			}

			Range = 4000f;
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
		}

		protected override void ServerTick()
		{
			base.ServerTick();

			if ( Team == Team.None || !IsPowered || !NextSensePlayers )
				return;

			var disruptors = Physics.GetEntitiesInSphere( Position, Range )
				.OfType<Disruptor>()
				.Where( IsEnemyDisruptor );

			if ( disruptors.Any() )
			{
				return;
			}

			var players = Physics.GetEntitiesInSphere( Position, Range )
				.OfType<Player>()
				.Where( IsValidTarget );

			var didFindPlayer = false;

			foreach ( var player in players )
			{
				if ( player.VisibleToEnemiesUntil )
				{
					player.VisibleToEnemiesUntil = 5f;
					didFindPlayer = true;
				}
			}

			if ( didFindPlayer )
			{
				PlaySound( "radar.beep" );
			}

			NextSensePlayers = 2f;
		}

		protected override void OnGeneratorRepaired( GeneratorAsset generator )
		{
			base.OnGeneratorRepaired( generator );

			PlayIdleSound();
		}

		protected override void OnGeneratorBroken( GeneratorAsset generator )
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

		private bool IsEnemyDisruptor( Disruptor disruptor )
		{
			return (disruptor.Team != Team);
		}

		private bool IsValidTarget( Player player )
		{
			if ( player.LifeState == LifeState.Dead )
				return false;

			if ( player.Team == Team )
				return false;

			var jammer = player.GetWeapon<RadarJammer>();

			if ( jammer.IsValid() && jammer.IsUsingAbility )
				return false;

			return true;
		}
	}
}
