using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class BarageConfig : WeaponConfig
	{
		public override string Name => "Barage";
		public override string Description => "Short-range grenade launcher";
		public override string Icon => "ui/weapons/barage.png";
		public override string ClassName => "hv_barage";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 20;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 400;
	}

	[Library( "hv_barage", Title = "Barage" )]
	partial class Barage : ProjectileWeapon<BouncingProjectile>
	{
		public override WeaponConfig Config => new BarageConfig();
		public override string ProjectileData => "barage";
		public override string ViewModelPath => "models/weapons/v_barage.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/grenade_launcher/grenade_launcher_muzzleflash.vpcf";
		public override string CrosshairClass => "shotgun";
		public override string DamageType => "blast";
		public override float InheritVelocity => 0.5f;
		public override float PrimaryRate => 2.0f;
		public override float SecondaryRate => 1.0f;
		public override bool CanMeleeAttack => true;
		public override int ClipSize => 3;
		public override float ReloadTime => 3f;
		public virtual float BlastRadius => 500f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_barage.vmdl" );
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
			PlaySound( $"barage.launch" );

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

		protected override void OnCreateProjectile( BouncingProjectile projectile )
		{
			projectile.Bounciness = 0.5f;

			base.OnCreateProjectile( projectile );
		}

		protected override float ModifyDamage( Entity victim, float damage )
		{
			if ( victim == Owner ) return damage * 1.25f;

			return base.ModifyDamage( victim, damage );
		}

		protected override void OnProjectileHit( Projectile projectile, Entity target )
		{
			ScreenShake.DoRandomShake( projectile.Position, BlastRadius, 2f );

			if ( Game.IsServer )
			{
				DamageInRadius( projectile.Position, BlastRadius, Config.Damage, 4f );
			}
		}
	}
}
