using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class DestroyerConfig : WeaponConfig
	{
		public override string Name => "Destroyer";
		public override string Description => "Slow medium-range slow mortar cannon";
		public override string SecondaryDescription => "+50% vs Structures";
		public override string Icon => "ui/weapons/destroyer.png";
		public override string ClassName => "hv_destroyer";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 10;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 1200;
	}

	[Library( "hv_destroyer", Title = "Destroyer" )]
	partial class Destroyer : ProjectileWeapon<DestroyerProjectile>
	{
		public override WeaponConfig Config => new DestroyerConfig();
		public override string ProjectileData => "destroyer";
		public override string MuzzleFlashEffect => "particles/weapons/destroyer/destroyer_muzzleflash.vpcf";
		public override string ViewModelPath => "models/weapons/v_barage.vmdl";
		public override int ViewModelMaterialGroup => 2;
		public override string CrosshairClass => "shotgun";
		public override float InheritVelocity => 0.5f;
		public override float PrimaryRate => 0.2f;
		public override float SecondaryRate => 1.0f;
		public override bool CanMeleeAttack => true;
		public override int ClipSize => 1;
		public override float ReloadTime => 4f;
		public virtual float BlastRadius => 800f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_barage.vmdl" );
			SetMaterialGroup( 2 );
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
			PlaySound( "destroyer.fire" );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		public override void PlayReloadSound()
		{
			PlaySound( "pulserifle.reload" );
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

		protected override void OnCreateProjectile( DestroyerProjectile projectile )
		{
			projectile.Bounciness = 1f;

			base.OnCreateProjectile( projectile );
		}

		protected override void OnProjectileHit( Projectile projectile, Entity target )
		{
			ScreenShake.DoRandomShake( projectile.Position, BlastRadius, 2f );

			if ( Game.IsServer )
			{
				Audio.Play( "explosion.far", projectile.Position );

				DamageInRadius( projectile.Position, BlastRadius, Config.Damage, 4f );
			}
		}
	}
}
