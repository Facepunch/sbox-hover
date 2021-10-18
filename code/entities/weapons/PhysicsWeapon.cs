using Sandbox;

namespace Facepunch.Hover
{
	public abstract partial class PhysicsWeapon<T> : Weapon where T : PhysicsProjectile, new()
	{
		public virtual float ProjectileForce => 1000f;
		public virtual string ProjectileModel => "";
		public virtual float InheritVelocity => 0f;
		public virtual float ImpactForce => 1000f;
		public virtual float DamageRadius => 500f;
		public virtual string TrailEffect => null;
		public virtual string HitSound => null;
		public virtual float LifeTime => 5f;

		public override void AttackPrimary()
		{
			if ( IsServer )
			{
				using ( Prediction.Off() )
                {
					FireProjectile();
				}
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

			var trace = Trace.Ray( position, position + forward * 80f )
				.Ignore( this )
				.Ignore( Owner )
				.Run();

			projectile.Position = trace.EndPos - trace.Direction * 40f;
			projectile.Rotation = Rotation.LookAt( forward );
			projectile.Initialize( OnProjectileHit );

			projectile.Velocity = Owner.Velocity * InheritVelocity;
			projectile.PhysicsBody.ApplyForce( forward * ProjectileForce * 200f );
		}

		protected virtual float ModifyDamage( Entity victim, float damage )
		{
			return damage;
		}

		protected virtual void DamageInRadius( Vector3 position, float radius, float baseDamage )
		{
			var entities = Physics.GetEntitiesInSphere( position, DamageRadius );

			foreach ( var entity in entities )
			{
				var direction = (entity.Position - position).Normal;
				var distance = entity.Position.Distance( position );
				var damage = baseDamage - ((baseDamage / radius) * distance);

				damage = ModifyDamage( entity, damage );

				DealDamage( entity, position, direction * ImpactForce * 0.4f, damage );
			}
		}

		protected virtual void OnProjectileHit( PhysicsProjectile projectile )
		{
			DamageInRadius( projectile.Position, DamageRadius, BaseDamage );
		}
	}
}
