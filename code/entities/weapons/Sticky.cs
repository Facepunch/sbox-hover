using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class StickyConfig : WeaponConfig
	{
		public override string Name => "Sticky";
		public override string Description => "Short-range sticky grenade launcher";
		public override string Icon => "ui/weapons/sticky.png";
		public override string ClassName => "hv_sticky";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override int Ammo => 9;
	}

	[Library( "hv_sticky", Title = "Sticky" )]
	partial class Sticky : PhysicsWeapon<StickyGrenade>
	{
		public override WeaponConfig Config => new StickyConfig();
		public override string ImpactEffect => "particles/weapons/sticky/sticky_impact.vpcf";
		public override string TrailEffect => "particles/weapons/sticky/sticky_projectile.vpcf";
		public override int ViewModelMaterialGroup => 1;
		public override string ViewModelPath => "models/weapons/v_barage.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/sticky/sticky_muzzleflash.vpcf";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override float PrimaryRate => 2.0f;
		public override float SecondaryRate => 1.0f;
		public override float ProjectileForce => 100f;
		public override float InheritVelocity => 0.5f;
		public override bool CanMeleeAttack => false;
		public override string ProjectileModel => "models/weapons/barage_grenade/barage_grenade.vmdl";
		public override float ImpactForce => 1000f;
		public override int ClipSize => 2;
		public override float ReloadTime => 3f;
		public override float LifeTime => 10f;
		public override float DamageRadius => 200;
		public override int BaseDamage => 800;

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

			ShootEffects();
			PlaySound( $"barage.launch" );

			AnimationOwner.SetAnimBool( "b_attack", true );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		public override bool CanReload()
		{
			return (TimeSincePrimaryAttack > 1f && AmmoClip == 0) || base.CanReload();
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

		protected override void OnProjectileHit( PhysicsProjectile projectile )
		{
			base.OnProjectileHit( projectile );
		}
	}
}
