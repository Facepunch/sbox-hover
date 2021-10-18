using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_light_turret" )]
	public partial class LightTurret : DeployableEntity, ITurretComponent, IKillFeedIcon
	{
		public override string Model => "models/deploy_turret/deploy_turret.vmdl";
		public override float MaxHealth => 800f;

		public List<string> FlybySounds => new()
		{
			"flyby.rifleclose1",
			"flyby.rifleclose2",
			"flyby.rifleclose3",
			"flyby.rifleclose4"
		};

		public string MuzzleAttachment => "muzzle";
		public float RotateSpeed => 10f;
		public float DamageFalloffStart => 1000f;
		public float DamageFalloffEnd => 2000f;
		public DamageFlags DamageType => DamageFlags.Bullet;
		public string MuzzleFlashEffect => "particles/weapons/deployable_turret/deployable_turret_muzzleflash.vpcf";
		public string TracerEffect => "particles/weapons/deployable_turret/deployable_turret_projectile.vpcf";
		public string ImpactEffect => "particles/weapons/deployable_turret/deployable_turret_impact.vpcf";
		public float BulletForce => 0.2f;
		public float BulletRange => 2000f;
		public float BaseDamage => 15f;
		public float TargetingSpeed => 1f;
		public float AttackRadius => 1000f;
		public float FireRate => 0.1f;

		public string GetKillFeedIcon()
		{
			return "ui/killicons/light_turret.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Light Turret";
		}

		public void FireProjectile( Player target, Vector3 direction )
		{
			ShootBullet( target, 0.3f, BulletForce, BaseDamage, 16f );
			PlaySound( $"generic.bullet1" );
		}

		public Vector3 GetTargetPosition( Player target )
		{
			return target.WorldSpaceBounds.Center;
		}

		public bool IsValidVictim( Player player )
		{
			return (player.LifeState == LifeState.Alive && player.Team != Team);
		}

		public override void Spawn()
		{
			base.Spawn();

			Components.Create<TurretComponent>();
		}

		public bool IsTurretDisabled()
		{
			return !FinishDeployTime;
		}

		public float GetDamageFalloff( float distance, float damage )
		{
			return WeaponUtil.GetDamageFalloff( distance, damage, DamageFalloffStart, DamageFalloffEnd );
		}

		protected void DealDamage( Entity target, Vector3 position, Vector3 force )
		{
			DealDamage( target, position, force, BaseDamage );
		}

		protected virtual void ShootBullet( Entity target, float spread, float force, float damage, float bulletSize )
		{
			var attachment = GetAttachment( "muzzle" );
			var startPosition = attachment.Value.Position;
			var direction = (target.WorldSpaceBounds.Center - startPosition).Normal;

			direction += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			direction = direction.Normal;

			var trace = Trace.Ray( startPosition, startPosition + direction * BulletRange )
				.Ignore( this )
				.Run();

			var fullEndPos = trace.EndPos;

			if ( !string.IsNullOrEmpty( TracerEffect ) )
			{
				var tracer = Particles.Create( TracerEffect );
				tracer.SetPosition( 0, startPosition );
				tracer.SetPosition( 1, fullEndPos );
			}

			if ( !string.IsNullOrEmpty( ImpactEffect ) )
			{
				var impact = Particles.Create( ImpactEffect, fullEndPos );
				impact.SetForward( 0, trace.Normal );
			}

			WeaponUtil.PlayFlybySounds( this, trace.Entity, trace.StartPos, trace.EndPos, bulletSize * 2f, bulletSize * 50f, FlybySounds );

			if ( trace.Entity.IsValid() )
			{
				damage = GetDamageFalloff( trace.Distance, damage );
				DealDamage( trace.Entity, trace.EndPos, trace.Normal * 100f * force, damage );
			}
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( !FinishDeployTime ) return;

			base.TakeDamage( info );
		}

		protected void DealDamage( Entity target, Vector3 position, Vector3 force, float damage )
		{
			var damageInfo = new DamageInfo()
				.WithAttacker( Owner )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( force )
				.WithFlag( DamageType );

			damageInfo.Damage = damage;

			target.TakeDamage( damageInfo );
		}

		[Event.Tick.Server]
		private void UpdateDeployment()
		{
			if ( !FinishDeployTime )
			{
				var timeLeft = FinishDeployTime.Relative;
				var fraction = 1f - (timeLeft / DeployTime);
				Health = MaxHealth * fraction;
			}
		}
	}
}
