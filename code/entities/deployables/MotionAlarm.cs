using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_motion_alarm" )]
	public partial class MotionAlarm : DeployableEntity
	{
		public override string ModelName => "models/motion_sensor/motion_sensor.vmdl";
		public string DamageType { get; set; } = "shock";
		public float BaseDamage { get; set; } = 20f;
		public float Radius { get; set; } = 300f;
		
		private RealTimeUntil NextSense { get; set; }

		public string GetKillFeedIcon()
		{
			return "ui/killicons/motion_alarm.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Motion Alarm";
		}

		public override void Spawn()
		{
			MaxHealth = 400f;

			base.Spawn();
		}

		protected virtual void DealDamage( HoverPlayer target, Vector3 position, Vector3 force, float damage )
		{
			var damageInfo = new DamageInfo()
				.WithAttacker( Deployer )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( force )
				.WithTag( DamageType );

			damageInfo.Damage = damage;

			target.TakeDamage( damageInfo );
		}

		protected virtual bool IsValidVictim( HoverPlayer victim )
		{
			return (victim.LifeState == LifeState.Alive && victim.Team != Team);
		}

		protected virtual void Sense( HoverPlayer target )
		{
			target.Energy = Math.Max( target.Energy - target.MaxEnergy * 0.4f, 0f );

			var force = (target.Position - Position).Normal * 100f * 1f;
			DealDamage( target, Position, force, BaseDamage );

			target.ShouldHideOnRadar = 5f;
		}

		protected override void ServerTick()
		{
			if ( !NextSense ) return;

			var players = Entity.FindInSphere( Position, Radius )
				.OfType<HoverPlayer>()
				.Where( IsValidVictim );

			var didSensePlayer = false;

			foreach ( var player in players )
			{
				if ( !didSensePlayer )
				{
					Particles.Create( "particles/generator/generator_attacked/generator_attacked.vpcf", this );
					PlaySound( "motion.alarm" );
					NextSense = 10f;
				}

				didSensePlayer = true;
				Sense( player );
			}

			base.ServerTick();
		}
	}
}
