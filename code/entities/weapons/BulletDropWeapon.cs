﻿using Gamelib.Utility;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	public abstract partial class BulletDropWeapon<T> : Weapon where T : BulletDropProjectile, new()
	{
		public virtual string ProjectileModel => "";
		public virtual float ProjectileRadius => 10f;
		public virtual float ProjectileLifeTime => 10f;
		public virtual string TrailEffect => null;
		public virtual string HitSound => null;
		public virtual float InheritVelocity => 0f;
		public virtual float Gravity => 50f;
		public virtual float Speed => 2000f;
		public virtual float Spread => 0.05f;

		public override void AttackPrimary()
		{
			if ( Prediction.FirstTime )
            {
				Game.SetRandomSeed( Time.Tick );
				FireProjectile();
            }
		}

		public virtual void FireProjectile()
		{
			if ( Owner is not HoverPlayer player )
				return;

			var projectile = new T()
			{
				ExplosionEffect = ImpactEffect,
				FaceDirection = true,
				IgnoreEntity = this,
				FlybySounds = FlybySounds,
				TrailEffect = TrailEffect,
				Simulator = player.Projectiles,
				Attacker = player,
				HitSound = HitSound,
				LifeTime = ProjectileLifeTime,
				Gravity = Gravity,
				ModelName = ProjectileModel
			};

			OnCreateProjectile( projectile );

			var muzzle = GetAttachment( MuzzleAttachment );
			var position = muzzle.Value.Position;
			var forward = player.EyeRotation.Forward;
			var endPosition = player.EyePosition + forward * BulletRange;
			var trace = Trace.Ray( player.EyePosition, endPosition )
				.Ignore( player )
				.Ignore( this )
				.Run();

			var direction = (trace.EndPosition - position).Normal;
			direction += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
			direction = direction.Normal;

			var velocity = (direction * Speed) + (player.Velocity * InheritVelocity);
			projectile.Initialize( position, velocity, ProjectileRadius, OnProjectileHit );
		}

		protected virtual float ModifyDamage( Entity victim, float damage )
		{
			return damage;
		}

		protected virtual void DamageInRadius( Vector3 position, float radius, float baseDamage, float force = 1f )
		{
			var entities = WeaponUtil.GetBlastEntities( position, radius );

			foreach ( var entity in entities )
			{
				var direction = (entity.Position - position).Normal;
				var distance = entity.Position.Distance( position );
				var damage = Math.Max( baseDamage - ((baseDamage / radius) * distance), 0f );

				damage = ModifyDamage( entity, damage );

				DealDamage( entity, position, direction * 100f * force, damage );
			}
		}

		protected virtual void OnCreateProjectile( T projectile )
		{

		}

		protected virtual void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			if ( Game.IsServer && target.IsValid() )
			{
				var distance = target.Position.Distance( projectile.StartPosition );
				var damage = GetDamageFalloff( distance, Config.Damage );
				DealDamage( target, projectile.Position, projectile.Velocity * 0.1f, damage );
			}
		}
	}
}
