using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class ShotblastConfig : WeaponConfig
	{
		public override string Name => "Shotblast";
		public override string Description => "Short-range hitscan shotgun";
		public override string Icon => "ui/weapons/shotblast.png";
		public override string ClassName => "hv_shotblast";
		public override AmmoType AmmoType => AmmoType.Shotgun;
		public override WeaponType Type => WeaponType.Hitscan;
		public override int Ammo => 30;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 100;
	}

	[Library( "hv_shotblast", Title = "Shotblast" )]
	partial class Shotblast : Weapon
	{
		public override WeaponConfig Config => new ShotblastConfig();
		public override string ImpactEffect => "particles/weapons/shotblast/shotblast_impact.vpcf";
		public override string TracerEffect => "particles/weapons/shotblast/shotblast_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/shotblast/shotblast_muzzleflash.vpcf";
		public override string ViewModelPath => "models/gameplay/weapons/shotblast/shotblast.vmdl";
		public override string CrosshairClass => "shotgun";
		public override float DamageFalloffStart => 1000f;
		public override float DamageFalloffEnd => 2000f;
		public override float PrimaryRate => 1;
		public override float SecondaryRate => 1;
		public override int ClipSize => 4;
		public override float ReloadTime => 2f;
		public override bool CanMeleeAttack => true;
		public virtual int BulletsPerFire => 8;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/gameplay/weapons/shotblast/w_shotblast.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			Game.SetRandomSeed( Time.Tick );

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"shotblast.fire{Game.Random.Int(1, 2)}" );

			for ( int i = 0; i < BulletsPerFire; i++ )
			{
				ShootBullet( 0.15f, 3f, Config.Damage, 4.0f );
			}

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
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
