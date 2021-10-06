﻿using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_pulsar", Title = "Pulsar" )]
	partial class Pulsar : ProjectileWeapon
	{
		public override string ImpactEffect => "particles/weapons/fusion_rifle/fusion_rifle_impact.vpcf";
		public override string TrailEffect => "particles/weapons/fusion_rifle/fusion_rifle_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_pulsar.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/fusion_rifle/fusion_rifle_muzzleflash.vpcf";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_smg.png" );
		public override string WeaponName => "Pulsar";
		public override float PrimaryRate => 1.0f;
		public override float SecondaryRate => 1.0f;
		public override int Slot => 4;
		public override int ClipSize => 1;
		public override bool ReloadAnimation => false;
		public override float ReloadTime => 1f;
		public override int BaseDamage => 200;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			ShootEffects();
			PlaySound( $"blaster.fire1" );

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