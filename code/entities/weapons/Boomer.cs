using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class BoomerConfig : WeaponConfig
	{
		public override string Name => "Boomer";
		public override string Description => "Short-range explosive projectile launcher";
		public override string Icon => "ui/weapons/boomer.png";
		public override string ClassName => "hv_boomer";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override int Ammo => 10;
	}

	[Library( "hv_boomer", Title = "Boomer" )]
	partial class Boomer : BulletDropWeapon
	{
		public override WeaponConfig Config => new BoomerConfig();
		public override string ImpactEffect => "particles/weapons/boomer/boomer_impact.vpcf";
		public override string TrailEffect => "particles/weapons/boomer/boomer_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_shotblast.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => "particles/weapons/boomer/boomer_muzzleflash.vpcf";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override DamageFlags DamageType => DamageFlags.Blast;
		public override float PrimaryRate => 0.3f;
		public override float SecondaryRate => 1.0f;
		public override float Speed => 1300f;
		public override float Gravity => 10f;
		public override float InheritVelocity => 0.5f;
		public override bool CanMeleeAttack => true;
		public override string ProjectileModel => "models/weapons/barage_grenade/barage_grenade.vmdl";
		public override int ClipSize => 1;
		public override float ReloadTime => 2.3f;
		public override float ProjectileLifeTime => 1.5f;
		public override int BaseDamage => 500;
		public virtual float BlastRadius => 300f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_shotblast.vmdl" );
			SetMaterialGroup( 1 );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"barage.launch" );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		public override void PlayReloadSound()
		{
			PlaySound( "blaster.reload" );
			base.PlayReloadSound();
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			if ( projectile == null || !projectile.IsValid() )
				return;

			var explosion = Particles.Create( "particles/weapons/boomer/boomer_explosion.vpcf" );

			if ( explosion == null )
				return;

			explosion.SetPosition( 0, projectile.Position - projectile.Velocity.Normal * projectile.Radius );

			var position = projectile.Position;
			var entities = WeaponUtil.GetBlastEntities( position, BlastRadius );

			foreach ( var entity in entities )
			{
				var direction = (entity.Position - position).Normal;
				var distance = entity.Position.Distance( position );
				var damage = BaseDamage - ((BaseDamage / BlastRadius) * distance);

				if ( entity == Owner )
				{
					damage *= 1.25f;
				}

				DealDamage( entity, position, direction * projectile.Velocity.Length * 0.2f, damage );
			}
		}
	}
}
