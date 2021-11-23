using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class RazorConfig : WeaponConfig
	{
		public override string Name => "Razor";
		public override string Description => "Short-range heat-seeking projectile weapon";
		public override string ClassName => "hv_razor";
		public override string Icon => "ui/weapons/razor.png";
		public override AmmoType AmmoType => AmmoType.Pistol;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 60;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 40;
	}

	[Library( "hv_razor", Title = "Razor" )]
	partial class Razor : BulletDropWeapon<RazorProjectile>
	{
		public override WeaponConfig Config => new RazorConfig();
		public override string ImpactEffect => "particles/weapons/razor/razor_impact.vpcf";
		public override string TrailEffect => "particles/weapons/razor/razor_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/razor/razor_muzzleflash.vpcf";
		public override string ViewModelPath => "models/weapons/v_sideman.vmdl";
		public override string CrosshairClass => "semiautomatic";
		public override int ClipSize => 8;
		public override float PrimaryRate => 12.0f;
		public override float ProjectileLifeTime => 3f;
		public override float DamageFalloffStart => 1000f;
		public override float DamageFalloffEnd => 4000f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 2.0f;
		public override bool CanMeleeAttack => true;
		public override int ViewModelMaterialGroup => 2;
		public override float Gravity => 0f;
		public override float Speed => 2000f;
		public override float Spread => 0.025f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_sideman.vmdl" );
			SetMaterialGroup( 2 );
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

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"generic.energy.fire2" );

			TimeSincePrimaryAttack = 0f;

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		protected override void CreateMuzzleFlash()
		{
			if ( !string.IsNullOrEmpty( MuzzleFlashEffect ) )
			{
				var effect = Particles.Create( MuzzleFlashEffect, GetEffectEntity(), "muzzle" );
				effect.SetPosition( 1, Color.Green * 255f );
			}
		}

		protected override void OnCreateProjectile( RazorProjectile projectile )
		{
			projectile.UpVelocity = Vector3.Up * 50f;
			projectile.SeekRadius = 300f;

			base.OnCreateProjectile( projectile );
		}
	}
}
