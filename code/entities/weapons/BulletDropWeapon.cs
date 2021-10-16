using Sandbox;

namespace Facepunch.Hover
{
	public abstract partial class BulletDropWeapon : Weapon
	{
		public virtual float ProjectileRadius => 20f;
		public virtual float ProjectileLifeTime => 10f;
		public virtual string TrailEffect => null;
		public virtual string HitSound => null;
		public virtual float InheritVelocity => 0f;
		public virtual float Gravity => 50f;
		public virtual float Speed => 2000f;
		public virtual float Spread => 0.05f;

		public override void AttackPrimary()
		{
			if ( IsServer )
			{
				FireProjectile();
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
				Gravity = Gravity
			};

			var muzzle = GetAttachment( MuzzleAttachment );
			var position = muzzle.Value.Position;
			var forward = Owner.EyeRot.Forward;
			var endPosition = Owner.EyePos + forward * BulletRange;
			var trace = Trace.Ray( Owner.EyePos, endPosition )
				.Ignore( Owner )
				.Ignore( this )
				.Run();
			var direction = (trace.EndPos - position).Normal;

			direction += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
			direction = direction.Normal;

			var velocity = (direction * Speed) + (Owner.Velocity * InheritVelocity);
			projectile.Initialize( position, velocity, ProjectileRadius, OnProjectileHit );
		}

		protected virtual void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			if ( target.IsValid() )
			{
				var distance = target.Position.Distance( projectile.StartPosition );
				var damage = GetDamageFalloff( distance, BaseDamage );
				DealDamage( target, projectile.Position, projectile.Velocity * 0.1f, damage );
			}
		}
	}
}
