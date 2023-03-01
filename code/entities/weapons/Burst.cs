using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public class BurstConfig : WeaponConfig
	{
		public override string Name => "Burst";
		public override string Description => "Medium-range hitscan burst-fire SMG";
		public override string ClassName => "hv_burst";
		public override string Icon => "ui/weapons/burst.png";
		public override AmmoType AmmoType => AmmoType.SMG;
		public override WeaponType Type => WeaponType.Hitscan;
		public override int Ammo => 72;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 80;
	}

	[Library( "hv_burst", Title = "Burst" )]
	partial class Burst : Weapon
	{
		public override WeaponConfig Config => new BurstConfig();
		public override string ImpactEffect => "particles/weapons/burst/burst_impact.vpcf";
		public override string TracerEffect => "particles/weapons/burst/burst_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/burst/burst_muzzleflash.vpcf";
		public override int ViewModelMaterialGroup => 1;
		public override string ViewModelPath => "models/gameplay/weapons/blaster/blaster.vmdl";
		public override string CrosshairClass => "semiautomatic";
		public override int ClipSize => 30;
		public override float PrimaryRate => 1.5f;
		public override float DamageFalloffStart => 2000f;
		public override float DamageFalloffEnd => 6000f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override bool CanMeleeAttack => true;
		public virtual int BulletsPerBurst => 3;

		[Net, Predicted] private TimeSince LastBulletTime { get; set; }
		[Net, Predicted] private bool FireBulletNow { get; set; }
		[Net, Predicted] private int BulletsToFire { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/gameplay/weapons/blaster/w_blaster.vmdl" );
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
			BulletsToFire += 3;
			FireBulletNow = true;
			AmmoClip = Math.Max( AmmoClip - BulletsPerBurst, 0 );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
		}

		public override void Simulate( IClient owner )
		{
			base.Simulate( owner );

			if ( BulletsToFire > 0 && (LastBulletTime > 0.075f || FireBulletNow) )
			{
				using ( LagCompensation() )
				{
					FireBulletNow = false;
					BulletsToFire--;

					Game.SetRandomSeed( Time.Tick );

					ShootEffects();
					PlaySound( $"generic.energy.fire3" );
					ShootBullet( 0.015f, 1.5f, Config.Damage, 8.0f );

					LastBulletTime = 0;
				}
			}
		}
	}
}
