using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library( "hv_barage", Title = "Barage" )]
	partial class Barage : PhysicsWeapon
	{
		public override WeaponConfig Config => new BarageConfig();
		public override string ImpactEffect => "particles/weapons/grenade_launcher/grenade_launcher_impact.vpcf";
		public override string TrailEffect => "particles/weapons/grenade_launcher/grenade_launcher_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_barage.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/grenade_launcher/grenade_launcher_muzzleflash.vpcf";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy )
		};
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override float PrimaryRate => 2.0f;
		public override float SecondaryRate => 1.0f;
		public override float ProjectileForce => 100f;
		public override bool CanMeleeAttack => false;
		public override string ProjectileModel => "models/weapons/barage_grenade/barage_grenade.vmdl";
		public override float ImpactForce => 1000f;
		public override int ClipSize => 3;
		public override float ReloadTime => 3f;
		public override float LifeTime => 2f;
		public override int BaseDamage => 150;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_barage.vmdl" );
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
