using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class PulsarConfig : WeaponConfig
	{
		public override string Name => "Pulsar";
		public override string Description => "Long-range explosive projectile based rifle";
		public override string Icon => "ui/weapons/pulsar.png";
		public override string ClassName => "hv_pulsar";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override int Ammo => 30;
	}

	[Library( "hv_pulsar", Title = "Pulsar" )]
	partial class Pulsar : BulletDropWeapon
	{
		public override WeaponConfig Config => new PulsarConfig();
		public override float ProjectileRadius => 20f;
		public override string ImpactEffect => "particles/weapons/fusion_rifle/fusion_rifle_impact.vpcf";
		public override string TrailEffect => "particles/weapons/fusion_rifle/fusion_rifle_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/fusion_rifle/fusion_rifle_muzzleflash.vpcf";
		public override string ViewModelPath => "models/weapons/v_pulsar.vmdl";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "semiautomatic";
		public override float InheritVelocity => 0.5f;
		public override string HitSound => "barage.explode";
		public override float PrimaryRate => 0.5f;
		public override float SecondaryRate => 1.0f;
		public override float Speed => 3500f;
		public override float Spread => 0f;
		public override DamageFlags DamageType => DamageFlags.Blast;
		public override int ClipSize => 1;
		public override bool ReloadAnimation => false;
		public override bool CanMeleeAttack => false;
		public override float ReloadTime => 1f;
		public override float Gravity => 0f;
		public override int BaseDamage => 600;
		public virtual float BlastRadius => 400f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_pulsar.vmdl" );
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
			PlaySound( $"pulserifle.fire{Rand.Int(1, 2)}" );

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
