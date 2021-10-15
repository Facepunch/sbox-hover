using Sandbox;

namespace Facepunch.Hover
{
	public abstract partial class PhysicsWeapon<T> : Weapon where T : PhysicsProjectile, new()
	{
		public virtual float ProjectileForce => 1000f;
		public virtual string ProjectileModel => "";
		public virtual float ImpactForce => 1000f;
		public virtual float DamageRadius => 500f;
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
			var projectile = new T()
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
			var entities = Physics.GetEntitiesInSphere( position, DamageRadius );

			foreach ( var entity in entities )
			{
				var direction = (entity.Position - position).Normal;
				var distance = entity.Position.Distance( position );
				var damage = BaseDamage - ((BaseDamage / DamageRadius) * distance);
				DealDamage( entity, position, direction * ImpactForce * 0.4f, damage );
			}
		}
	}
}
