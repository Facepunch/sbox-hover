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
	partial class Boomer : PhysicsWeapon<BoomerProjectile>
	{
		public override WeaponConfig Config => new BoomerConfig();
		public override string ImpactEffect => "particles/weapons/grenade_launcher/grenade_launcher_impact.vpcf";
		public override string TrailEffect => "particles/weapons/grenade_launcher/grenade_launcher_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_shotblast.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => "particles/weapons/grenade_launcher/grenade_launcher_muzzleflash.vpcf";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override float PrimaryRate => 0.3f;
		public override float SecondaryRate => 1.0f;
		public override float ProjectileForce => 150f;
		public override bool CanMeleeAttack => false;
		public override string ProjectileModel => "models/weapons/barage_grenade/barage_grenade.vmdl";
		public override float ImpactForce => 1000f;
		public override int ClipSize => 1;
		public override float ReloadTime => 2.3f;
		public override float LifeTime => 1.2f;
		public override int BaseDamage => 500;
		public override float DamageRadius => 300f;

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
	}
}
