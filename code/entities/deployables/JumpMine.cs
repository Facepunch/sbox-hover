using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_jump_mine" )]
	public partial class JumpMine : DeployableEntity, IKillFeedIcon
	{
		public override PhysicsMotionType MotionType => PhysicsMotionType.Dynamic;
		public override bool RequiresPower => false;
		public override bool StartFrozen => true;
		public override string Model => "models/mines/mines.vmdl";
		public override float MaxHealth { get; set; } = 80f;
		public DamageFlags DamageType { get; set; } = DamageFlags.Blast;
		public float BaseDamage { get; set; } = 700f;
		public float DamageVsHeavy { get; set; } = 1f;
		public float Radius { get; set; } = 150f;
		
		private RealTimeUntil ExplodeTime { get; set; }
		private bool IsExploding { get; set; }

		public string GetKillFeedIcon()
		{
			return "ui/killicons/jump_mine.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Jump Mine";
		}

		protected virtual void DealDamage( Vector3 position, float damage )
		{
			var players = WeaponUtil.GetBlastEntities<Player>( Position, Radius * 1.25f )
				.Where( IsValidVictim );

			foreach ( var player in players )
			{
				var force = (player.Position - Position).Normal * 100f * 3f;

				var damageInfo = new DamageInfo()
					.WithAttacker( Deployer )
					.WithWeapon( this )
					.WithPosition( position )
					.WithForce( force )
					.WithFlag( DamageType );

				damageInfo.Damage = damage;

				if ( player.Loadout.ArmorType == LoadoutArmorType.Heavy )
				{
					damageInfo.Damage *= DamageVsHeavy;
				}

				player.TakeDamage( damageInfo );
			}
		}

		protected virtual bool IsValidTarget( Player victim )
		{
			return (victim.LifeState == LifeState.Alive && victim.Team != Team);
		}

		protected virtual bool IsValidVictim( Player victim )
		{
			return (victim.LifeState == LifeState.Alive && (victim == Deployer || victim.Team != Team));
		}

		protected virtual void Explode()
		{
			PhysicsEnabled = true;
			PhysicsBody.ApplyImpulse( Vector3.Up * 10f * 3000f );
			PlaySound( "sticky.warning" );
			IsExploding = true;
			ExplodeTime = 0.3f;
		}

		protected override void ServerTick()
		{
			if ( IsExploding )
			{
				if ( ExplodeTime )
				{
					DealDamage( Position, BaseDamage );
					OnKilled();
				}

				return;
			}

			var players = WeaponUtil.GetBlastEntities<Player>( Position, Radius )
				.Where( IsValidTarget );

			if ( players.Any() )
			{
				Explode();
			}

			base.ServerTick();
		}
	}
}
