using Sandbox;

namespace Facepunch.Hover
{
	partial class ProjectileWeapon : Weapon
	{
		public virtual string TrailEffect => null;
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
			var projectile = new PhysicsProjectile()
			{
				ExplosionEffect = ImpactEffect,
				IgnoreEntity = this,
				TrailEffect = TrailEffect,
				LifeTime = 10f,
				Gravity = 50f,
				Owner = Owner
			};

			var muzzle = GetAttachment( MuzzleAttachment );
			var position = muzzle.Value.Position;
			var forward = Owner.EyeRot.Forward;

			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
			forward = forward.Normal;

			projectile.Initialize( position, forward, 20f, Speed, OnProjectileHit );
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

		protected virtual void OnProjectileHit( PhysicsProjectile projectile, Entity target )
		{
			DealDamage( target, projectile.Position, projectile.Direction * projectile.Speed * 0.1f );
		}

		protected void DealDamage( Entity target, Vector3 position, Vector3 force, float damageScale = 1f )
		{
			var damageInfo = new DamageInfo()
				.WithAttacker( Owner )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( force )
				.WithFlag( DamageType );

			damageInfo.Damage = BaseDamage * damageScale;

			target.TakeDamage( damageInfo );
		}
	}
}
