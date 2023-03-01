using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class BlasterConfig : WeaponConfig
	{
		public override string Name => "Blaster";
		public override string Description => "Medium-range projectile plasma SMG";
		public override string Icon => "ui/weapons/blaster.png";
		public override string ClassName => "hv_blaster";
		public override AmmoType AmmoType => AmmoType.SMG;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 90;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 120;
	}

	[Library( "hv_blaster", Title = "Blaster" )]
	partial class Blaster : ProjectileWeapon<Projectile>
	{
		public override WeaponConfig Config => new BlasterConfig();
		public override string ProjectileData => "blaster";
		public override string ViewModelPath => "models/gameplay/weapons/blaster/blaster.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/blaster/blaster_muzzleflash.vpcf";
		public override string CrosshairClass => "automatic";
		public override float PrimaryRate => 7.5f;
		public override float DamageFalloffStart => 1500f;
		public override float DamageFalloffEnd => 4000f;
		public override float InheritVelocity => 0.5f;
		public override float SecondaryRate => 1.0f;
		public override bool CanMeleeAttack => true;
		public override int ClipSize => 30;
		public override float Spread => 0.05f;
		public override bool ReloadAnimation => true;
		public override float ReloadTime => 3f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/gameplay/weapons/blaster/w_blaster.vmdl" );
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
			PlaySound( $"blaster.fire1" );

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
	}
}
