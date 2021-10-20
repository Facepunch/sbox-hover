using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class BurstConfig : WeaponConfig
	{
		public override string Name => "Burst";
		public override string Description => "Medium-range hitscan burst-fire SMG";
		public override string ClassName => "hv_burst";
		public override string Icon => "ui/weapons/burst.png";
		public override AmmoType AmmoType => AmmoType.SMG;
		public override int Ammo => 72;
	}

	[Library( "hv_burst", Title = "Burst" )]
	partial class Burst : Weapon
	{
		public override WeaponConfig Config => new BurstConfig();
		public override string ImpactEffect => "particles/weapons/burst/burst_impact.vpcf";
		public override string TracerEffect => "particles/weapons/burst/burst_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/burst/burst_muzzleflash.vpcf";
		public override int ViewModelMaterialGroup => 1;
		public override string ViewModelPath => "models/weapons/v_blaster.vmdl";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "semiautomatic";
		public override int ClipSize => 24;
		public override float PrimaryRate => 2.0f;
		public override float DamageFalloffStart => 1500f;
		public override float DamageFalloffEnd => 4000f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override int BaseDamage => 120;
		public override bool CanMeleeAttack => false;
		public virtual int BulletsPerBurst => 3;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_blaster.vmdl" );
			SetMaterialGroup( 1 );
		}

		public override void PlayReloadSound()
		{
			PlaySound( "blaster.reload" );
			base.PlayReloadSound();
		}

		public override void AttackPrimary()
		{
			if ( AmmoClip == 0 )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			TimeSincePrimaryAttack = 0f;

			FireBurst( Math.Min( AmmoClip, BulletsPerBurst ) );
			AmmoClip = Math.Max( AmmoClip - BulletsPerBurst, 0 );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
		}

		private async void FireBurst( int bulletsToFire )
		{
			Rand.SetSeed( Time.Tick );

			for ( var i = 0; i < bulletsToFire; i++ )
			{
				if ( !IsValid ) return;

				ShootEffects();
				PlaySound( $"generic.bullet1" );
				ShootBullet( 0.025f, 1.5f, BaseDamage, 4.0f );

				await GameTask.Delay( Rand.Int( 30, 50 ) );
			}
		} 
	}
}
