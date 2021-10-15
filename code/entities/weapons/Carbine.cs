﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class CarbineConfig : WeaponConfig
	{
		public override string Name => "Carbine";
		public override string Description => "Medium-range hitscan assault rifle";
		public override string ClassName => "hv_carbine";
		public override string Icon => "ui/weapons/carbine.png";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override int Ammo => 90;
	}

	[Library( "hv_carbine", Title = "Carbine" )]
	partial class Carbine : Weapon
	{
		public override WeaponConfig Config => new CarbineConfig();
		public override string ImpactEffect => "particles/weapons/carbine/carbine_impact.vpcf";
		public override string TracerEffect => "particles/weapons/carbine/carbine_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/carbine/carbine_muzzleflash.vpcf";
		public override int ViewModelMaterialGroup => 2;
		public override string ViewModelPath => "models/weapons/v_blaster.vmdl";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "semiautomatic";
		public override int ClipSize => 20;
		public override float PrimaryRate => 7f;
		public override float DamageFalloffStart => 2000f;
		public override float DamageFalloffEnd => 6000f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override int BaseDamage => 40;
		public override bool CanMeleeAttack => false;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_blaster.vmdl" );
			SetMaterialGroup( 2 );
		}

		public override void PlayReloadSound()
		{
			PlaySound( "blaster.reload" );
			base.PlayReloadSound();
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			TimeSincePrimaryAttack = 0f;

			ShootEffects();
			PlaySound( $"generic.bullet1" );
			ShootBullet( 0.025f, 1.5f, BaseDamage, 4.0f );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
		}
	}
}
