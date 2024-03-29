﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class BoomerConfig : WeaponConfig
	{
		public override string Name => "Boomer";
		public override string Description => "Short-range explosive projectile launcher";
		public override string Icon => "ui/weapons/boomer.png";
		public override string ClassName => "hv_boomer";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 10;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 500;
	}

	[Library( "hv_boomer", Title = "Boomer" )]
	partial class Boomer : ProjectileWeapon<Projectile>
	{
		public override WeaponConfig Config => new BoomerConfig();
		public override string ProjectileData => "boomer";
		public override string ViewModelPath => "models/gameplay/weapons/shotblast/shotblast.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => "particles/weapons/boomer/boomer_muzzleflash.vpcf";
		public override string CrosshairClass => "shotgun";
		public override string DamageType => "blast";
		public override float PrimaryRate => 0.3f;
		public override float SecondaryRate => 1.0f;
		public override float InheritVelocity => 0.7f;
		public override bool CanMeleeAttack => true;
		public override int ClipSize => 1;
		public override float ReloadTime => 2.3f;
		public virtual float BlastRadius => 300f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/gameplay/weapons/shotblast/w_shotblast.vmdl" );
			SetMaterialGroup( 1 );
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

		protected override float ModifyDamage( Entity victim, float damage )
		{
			if ( victim == Owner ) return damage * 1.25f;

			return base.ModifyDamage( victim, damage );
		}

		protected override void OnProjectileHit( Projectile projectile, Entity target )
		{
			var explosion = Particles.Create( "particles/weapons/boomer/boomer_explosion.vpcf" );
			explosion.SetPosition( 0, projectile.Position - projectile.Velocity.Normal * projectile.Data.Radius );

			if ( Game.IsServer )
            {
				DamageInRadius( projectile.Position, BlastRadius, Config.Damage, 4f );
			}
		}
	}
}
