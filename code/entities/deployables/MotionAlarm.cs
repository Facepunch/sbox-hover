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
		public override string Model => "models/motion_sensor/motion_sensor.vmdl";
		public override float MaxHealth => 300f;
		public DamageFlags DamageType { get; set; } = DamageFlags.Shock;
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

		protected virtual void DealDamage( Player target, Vector3 position, Vector3 force, float damage )
		{
			var damageInfo = new DamageInfo()
				.WithAttacker( Deployer )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( force )
				.WithFlag( DamageType );

			damageInfo.Damage = damage;

			target.TakeDamage( damageInfo );
		}

		protected virtual bool IsValidVictim( Player victim )
		{
			return (victim.LifeState == LifeState.Alive && victim.Team != Team);
		}

		protected virtual void Sense( Player target )
		{
			if ( target.Controller is MoveController controller )
			{
				controller.Energy = Math.Max( controller.Energy - 50, 0f );
			}

			var force = (target.Position - Position).Normal * 100f * 1f;
			DealDamage( target, Position, force, BaseDamage );

			target.VisibleToEnemiesUntil = 5f;
		}

		protected override void ServerTick()
		{
			if ( !NextSense ) return;

			var players = Physics.GetEntitiesInSphere( Position, Radius )
				.OfType<Player>()
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
