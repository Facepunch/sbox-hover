using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class DestroyerConfig : WeaponConfig
	{
		public override string Name => "Destroyer";
		public override string Description => "Slow medium-range slow mortar cannon";
		public override string SecondaryDescription => "+50% vs Structures";
		public override string Icon => "ui/weapons/destroyer.png";
		public override string ClassName => "hv_destroyer";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override int Ammo => 20;
	}

	[Library( "hv_destroyer", Title = "Destroyer" )]
	partial class Destroyer : BulletDropWeapon
	{
		public override WeaponConfig Config => new DestroyerConfig();
		public override float ProjectileRadius => 50f;
		public override string ImpactEffect => "particles/weapons/fusion_rifle/fusion_rifle_impact.vpcf";
		public override string TrailEffect => "particles/weapons/fusion_rifle/fusion_rifle_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/fusion_rifle/fusion_rifle_muzzleflash.vpcf";
		public override string ViewModelPath => "models/weapons/v_barage.vmdl";
		public override int ViewModelMaterialGroup => 2;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override float InheritVelocity => 0.5f;
		public override float PrimaryRate => 0.5f;
		public override float SecondaryRate => 1.0f;
		public override bool CanMeleeAttack => false;
		public override float Speed => 600f;
		public override int ClipSize => 1;
		public override float ReloadTime => 3f;
		public override int BaseDamage => 1500;
		public override float Gravity => 15f;
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
			PlaySound( $"pulserifle.fire{Rand.Int( 1, 2 )}" );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		public override void PlayReloadSound()
		{
			PlaySound( "pulserifle.reload" );
			base.PlayReloadSound();
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			var explosion = Particles.Create( "particles/weapons/fusion_rifle/fusion_rifle_explosion.vpcf" );
			explosion.SetPosition( 0, projectile.Position - projectile.Velocity.Normal * projectile.Radius );

			var position = projectile.Position;
			var entities = Physics.GetEntitiesInSphere( position, BlastRadius );

			foreach ( var entity in entities )
			{
				var direction = (entity.Position - position).Normal;
				var distance = entity.Position.Distance( position );
				var damage = BaseDamage - ((BaseDamage / BlastRadius) * distance);
				DealDamage( entity, position, direction * projectile.Velocity.Length * 0.25f, damage );
			}
		}
	}
}
