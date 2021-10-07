using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library( "hv_sideman", Title = "Sideman" )]
	partial class Sideman : Weapon
	{
		public override string ImpactEffect => "particles/weapons/pulse_sniper/pulse_sniper_impact.vpcf";
		public override string TracerEffect => "particles/weapons/pulse_sniper/pulse_sniper_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/pulse_sniper/pulse_sniper_muzzleflash.vpcf";
		public override string ViewModelPath => "models/weapons/v_sideman.vmdl";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_pistol.png" );
		public override string WeaponName => "Sideman";
		public override int ClipSize => 15;
		public override float PrimaryRate => 15.0f;
		public override AmmoType AmmoType => AmmoType.Pistol;
		public override float DamageFalloffStart => 500f;
		public override float DamageFalloffEnd => 2000f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 2.0f;
		public override int BaseDamage => 50;
		public override bool CanMeleeAttack => false;
		public override int Slot => 2;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_sideman.vmdl" );
		}

		public override bool CanPrimaryAttack()
		{
			return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			ShootEffects();
			PlaySound( $"sideman.fire{Rand.Int(1, 2)}" );
			ShootBullet( 0.05f, 1.5f, BaseDamage, 3.0f );

			TimeSincePrimaryAttack = 0f;

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
		}
	}
}
