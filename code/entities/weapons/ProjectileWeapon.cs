using Sandbox;
using System;

namespace Facepunch.Hover
{
	public abstract partial class ProjectileWeapon<T> : Weapon where T : Projectile, new()
	{
		public virtual string ProjectileData => "";
		public virtual float InheritVelocity => 0f;
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

			if ( string.IsNullOrEmpty( ProjectileData ) )
			{
				throw new Exception( $"Projectile Data has not been set for {this}!" );
			}

			var projectile = Projectile.Create<T>( ProjectileData );

			projectile.IgnoreEntity = this;
			projectile.FlybySounds = FlybySounds;
			projectile.Simulator = player.Projectiles;
			projectile.Attacker = player;

			OnCreateProjectile( projectile );

			var muzzle = GetAttachment( MuzzleAttachment );
			var position = muzzle.Value.Position.WithZ( MathF.Max( muzzle.Value.Position.z, player.EyePosition.z ) );
			var forward = player.EyeRotation.Forward;
			var endPosition = player.EyePosition + forward * BulletRange;
			var trace = Trace.Ray( player.EyePosition, endPosition )
				.Ignore( player )
				.Ignore( this )
				.Run();

			var direction = (trace.EndPosition - position).Normal;
			direction += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
			direction = direction.Normal;

			var speed = projectile.Data.Speed.GetValue();
			var velocity = (direction * speed) + (player.Velocity * InheritVelocity);
			projectile.Initialize( position, velocity, OnProjectileHit );
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

		protected virtual void OnProjectileHit( Projectile projectile, Entity target )
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
