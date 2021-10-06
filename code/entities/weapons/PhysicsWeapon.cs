using Sandbox;

namespace Facepunch.Hover
{
	partial class PhysicsWeapon : Weapon
	{
		public virtual float ProjectileForce => 1000f;
		public virtual string ProjectileModel => "";
		public virtual float ImpactForce => 1000f;
		public virtual string TrailEffect => null;
		public virtual string HitSound => null;
		public virtual float LifeTime => 5f;

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
				TrailEffect = TrailEffect,
				HitSound = HitSound,
				LifeTime = LifeTime,
				Owner = Owner
			};

			projectile.SetModel( ProjectileModel );

			var muzzle = GetAttachment( MuzzleAttachment );
			var position = muzzle.Value.Position;
			var forward = Owner.EyeRot.Forward.Normal;

			projectile.Position = position + forward * 50f;
			projectile.Rotation = Rotation.LookAt( forward );
			projectile.Initialize( OnProjectileHit );
			projectile.PhysicsBody.ApplyForce( forward * ProjectileForce * 200f );
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

		protected virtual void OnProjectileHit( PhysicsProjectile projectile )
		{
			var position = projectile.Position;
			var entities = Physics.GetEntitiesInSphere( position, 500f );

			foreach ( var entity in entities )
			{
				var direction = (entity.Position - position).Normal;
				DealDamage( entity, position, direction * ImpactForce * 0.4f );
			}
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
