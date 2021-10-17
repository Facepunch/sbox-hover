using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class ShotblastConfig : WeaponConfig
	{
		public override string Name => "Shotblast";
		public override string Description => "Short-range hitscan shotgun";
		public override string Icon => "ui/weapons/shotblast.png";
		public override string ClassName => "hv_shotblast";
		public override AmmoType AmmoType => AmmoType.Shotgun;
		public override int Ammo => 30;
	}

	[Library( "hv_shotblast", Title = "Shotblast" )]
	partial class Shotblast : Weapon
	{
		public override WeaponConfig Config => new ShotblastConfig();
		public override string ImpactEffect => "particles/weapons/shotblast/shotblast_impact.vpcf";
		public override string TracerEffect => "particles/weapons/shotblast/shotblast_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/shotblast/shotblast_muzzleflash.vpcf";
		public override string ViewModelPath => "models/weapons/v_shotblast.vmdl";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "shotgun";
		public override float DamageFalloffStart => 0f;
		public override float DamageFalloffEnd => 1000f;
		public override float PrimaryRate => 1;
		public override float SecondaryRate => 1;
		public override int ClipSize => 4;
		public override float ReloadTime => 2f;
		public override bool CanMeleeAttack => false;
		public override int BaseDamage => 80;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_shotblast.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			Rand.SetSeed( Time.Tick );

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"shotblast.fire{Rand.Int(1, 2)}" );

			for ( int i = 0; i < 6; i++ )
			{
				ShootBullet( 0.5f, 3f, BaseDamage, 3.0f );
			}

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
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
