﻿using Sandbox;

namespace Facepunch.Hover
{
	public partial class BulletDropWeapon : Weapon
	{
		public virtual float ProjectileRadius => 20f;
		public virtual float ProjectileLifeTime => 10f;
		public virtual string TrailEffect => null;
		public virtual string HitSound => null;
		public virtual float Gravity => 50f;
		public virtual float Speed => 2000f;
		public virtual float Spread => 0.05f;

		protected bool FireNextTick { get; set; }

		public override void AttackPrimary()
		{
			if ( IsServer )
			{
				FireNextTick = true;
			}
		}

		public virtual void FireProjectile()
		{
			var projectile = new BulletDropProjectile()
			{
				ExplosionEffect = ImpactEffect,
				IgnoreEntity = this,
				FlybySounds = FlybySounds,
				TrailEffect = TrailEffect,
				Attacker = Owner,
				HitSound = HitSound,
				LifeTime = ProjectileLifeTime,
				Gravity = Gravity,
				Owner = Owner
			};

			var muzzle = GetAttachment( MuzzleAttachment );
			var position = muzzle.Value.Position;
			var forward = Owner.EyeRot.Forward;

			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
			forward = forward.Normal;

			projectile.Initialize( position, forward, ProjectileRadius, Speed, OnProjectileHit );
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( FireNextTick )
			{
				FireProjectile();
				FireNextTick = false;
			}
		}

		protected virtual void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			if ( target.IsValid() )
			{
				var distance = target.Position.Distance( projectile.StartPosition );
				var damage = GetDamageFalloff( distance, BaseDamage );
				DealDamage( target, projectile.Position, projectile.Direction * projectile.Speed * 0.1f, damage );
			}
		}
	}
}
