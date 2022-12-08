using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class StickyConfig : WeaponConfig
	{
		public override string Name => "Sticky";
		public override string Description => "Short-range sticky grenade launcher";
		public override string Icon => "ui/weapons/sticky.png";
		public override string ClassName => "hv_sticky";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override int Ammo => 9;
		public override WeaponType Type => WeaponType.Projectile;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 800;
	}

	[Library( "hv_sticky", Title = "Sticky" )]
	partial class Sticky : BulletDropWeapon<StickyGrenade>
	{
		public override WeaponConfig Config => new StickyConfig();
		public override string ImpactEffect => "particles/weapons/sticky/sticky_impact.vpcf";
		public override string TrailEffect => "particles/weapons/sticky/sticky_projectile.vpcf";
		public override int ViewModelMaterialGroup => 1;
		public override string ViewModelPath => "models/weapons/v_barage.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/sticky/sticky_muzzleflash.vpcf";
		public override DamageFlags DamageType => DamageFlags.Blast;
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override float PrimaryRate => 2.0f;
		public override float SecondaryRate => 1.0f;
		public override float ProjectileLifeTime => 10f;
		public override float InheritVelocity => 0.5f;
		public override bool CanMeleeAttack => true;
		public override string ProjectileModel => "models/weapons/barage_grenade/barage_grenade.vmdl";
		public override int ClipSize => 2;
		public override float ReloadTime => 3f;
		public override float Gravity => 35f;
		public virtual float BlastRadius => 200f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_barage.vmdl" );
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

		public override void SimulateAnimator( AnimationHelperWithLegs anim )
		{
			anim.HoldType = AnimationHelperWithLegs.HoldTypes.Rifle;
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			if ( IsServer )
			{
				DamageInRadius( projectile.Position, BlastRadius, Config.Damage, 4f );
			}
		}
	}
}
