using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class SidemanConfig : WeaponConfig
	{
		public override string Name => "Sideman";
		public override string Description => "Short-range hitscan pistol";
		public override string ClassName => "hv_sideman";
		public override string Icon => "ui/weapons/sideman.png";
		public override AmmoType AmmoType => AmmoType.Pistol;
		public override int Ammo => 60;
	}

	[Library( "hv_sideman", Title = "Sideman" )]
	partial class Sideman : Weapon
	{
		public override WeaponConfig Config => new SidemanConfig();
		public override string ImpactEffect => "particles/weapons/sideman/sideman_impact.vpcf";
		public override string TracerEffect => "particles/weapons/sideman/sideman_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/sideman/sideman_muzzleflash.vpcf";
		public override string ViewModelPath => "models/weapons/v_sideman.vmdl";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "semiautomatic";
		public override int ClipSize => 15;
		public override float PrimaryRate => 12.0f;
		public override float DamageFalloffStart => 500f;
		public override float DamageFalloffEnd => 2000f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 2.0f;
		public override int BaseDamage => 60;
		public override bool CanMeleeAttack => true;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_sideman.vmdl" );
		}

		public override bool CanPrimaryAttack()
		{
			return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
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

			Rand.SetSeed( Time.Tick );

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"generic.energy.fire2" );
			ShootBullet( 0.05f, 1.5f, BaseDamage, 3.0f );

			TimeSincePrimaryAttack = 0f;

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
		}
	}
}
