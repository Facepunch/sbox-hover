using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_light_turret" )]
	public partial class LightTurret : DeployableEntity
	{
		public override string Model => "models/deploy_turret/deploy_turret.vmdl";
		public override float MaxHealth => 800f;

		[Net] public Vector3 TargetDirection { get; private set; }
		[Net] public float Recoil { get; private set; }
		[Net] public Player Target { get; set; }

		public List<string> FlybySounds => new()
		{
			"flyby.rifleclose1",
			"flyby.rifleclose2",
			"flyby.rifleclose3",
			"flyby.rifleclose4"
		};

		public float DamageFalloffStart => 1000f;
		public float DamageFalloffEnd => 2000f;
		public RealTimeUntil NextFindTarget { get; set; }
		public RealTimeUntil NextFireTime { get; set; }
		public DamageFlags DamageType => DamageFlags.Bullet;
		public string MuzzleFlashEffect => "particles/weapons/deployable_turret/deployable_turret_muzzleflash.vpcf";
		public string TracerEffect => "particles/weapons/deployable_turret/deployable_turret_projectile.vpcf";
		public string ImpactEffect => "particles/weapons/deployable_turret/deployable_turret_impact.vpcf";
		public float BulletForce => 0.2f;
		public float BulletRange => 2000f;
		public float RotateSpeed => 10f;
		public float AttackRadius => 1000f;
		public float LockOnTime => 1f;
		public float BaseDamage => 15f;
		public float FireRate => 0.2f;

		private Vector3 ClientDirection { get; set; }

		public float GetDamageFalloff( float distance, float damage )
		{
			return WeaponUtil.GetDamageFalloff( distance, damage, DamageFalloffStart, DamageFalloffEnd );
		}

		protected void DealDamage( Entity target, Vector3 position, Vector3 force )
		{
			DealDamage( target, position, force, BaseDamage );
		}

		protected virtual void ShootBullet( Entity target, float spread, float force, float damage, float bulletSize )
		{
			var attachment = GetAttachment( "muzzle" );
			var startPosition = attachment.Value.Position;
			var direction = (target.WorldSpaceBounds.Center - startPosition).Normal;

			direction += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			direction = direction.Normal;

			var trace = Trace.Ray( startPosition, startPosition + direction * BulletRange )
				.Ignore( this )
				.Run();

			var fullEndPos = trace.EndPos;

			if ( !string.IsNullOrEmpty( TracerEffect ) )
			{
				var tracer = Particles.Create( TracerEffect );
				tracer.SetPosition( 0, startPosition );
				tracer.SetPosition( 1, fullEndPos );
			}

			if ( !string.IsNullOrEmpty( ImpactEffect ) )
			{
				var impact = Particles.Create( ImpactEffect, fullEndPos );
				impact.SetForward( 0, trace.Normal );
			}

			WeaponUtil.PlayFlybySounds( this, trace.Entity, trace.StartPos, trace.EndPos, bulletSize * 2f, bulletSize * 50f, FlybySounds );

			if ( trace.Entity.IsValid() )
			{
				damage = GetDamageFalloff( trace.Distance, damage );
				DealDamage( trace.Entity, trace.EndPos, trace.Normal * 100f * force, damage );
			}
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( !FinishDeployTime ) return;

			base.TakeDamage( info );
		}

		protected void DealDamage( Entity target, Vector3 position, Vector3 force, float damage )
		{
			var damageInfo = new DamageInfo()
				.WithAttacker( Owner )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( force )
				.WithFlag( DamageType );

			damageInfo.Damage = damage;

			target.TakeDamage( damageInfo );
		}

		private void FindClosestTarget()
		{
			var targets = Physics.GetEntitiesInSphere( Position, AttackRadius )
				.OfType<Player>()
				.Where( IsValidTarget );

			var closestTarget = (Player)null;
			var closestDistance = 0f;

			foreach ( var target in targets )
			{
				var distance = target.Position.Distance( Position );

				if ( !closestTarget.IsValid() || distance < closestDistance )
				{
					closestTarget = target;
					closestDistance = distance;
				}
			}

			if ( closestTarget != Target )
			{
				NextFireTime = LockOnTime;
			}

			if ( closestTarget.IsValid() )
				Target = closestTarget;
			else
				Target = null;
		}

		[Event.Tick.Server]
		private void UpdateTarget()
		{
			if ( !FinishDeployTime )
			{
				var timeLeft = FinishDeployTime.Relative;
				var fraction = 1f - (timeLeft / DeployTime);
				Health = MaxHealth * fraction;
				return;
			}

			if ( IsPowered )
			{
				if ( NextFindTarget )
				{
					FindClosestTarget();
					NextFindTarget = 0.1f;
				}

				if ( Target.IsValid() && IsValidTarget( Target ) )
				{
					TargetDirection = (Target.WorldSpaceBounds.Center - Position).Normal;

					if ( NextFireTime && IsFacingTarget() )
					{
						FireProjectile();
						NextFireTime = FireRate;
					}
				}
				else
				{
					TargetDirection = Vector3.Zero;
					Target = null;
				}
			}
			else
			{
				var positionAhead = (Position + Rotation.Forward * 500f) + Vector3.Down * 200f;
				TargetDirection = (positionAhead - Position).Normal;
			}

			UpdateAnimation();

			Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
		}

		protected override void ClientTick()
		{
			base.ClientTick();

			UpdateAnimation();
		}

		private void UpdateAnimation()
		{
			ClientDirection = ClientDirection.LerpTo( TargetDirection, Time.Delta * RotateSpeed );

			SetAnimFloat( "fire", Recoil );
			SetAnimVector( "target", Transform.NormalToLocal( ClientDirection ) );
			SetAnimFloat( "weight", 1f );
		}

		private void FireProjectile()
		{
			if ( !Target.IsValid() ) return;

			if ( !string.IsNullOrEmpty( MuzzleFlashEffect ) )
			{
				Particles.Create( MuzzleFlashEffect, this, "muzzle" );
			}

			ShootBullet( Target, 0.3f, BulletForce, BaseDamage, 16f );
			PlaySound( $"generic.bullet1" );

			Recoil = 1f;
		}

		private bool IsValidTarget( Player player )
		{
			return (IsValidVictim( player ) && CanSeeTarget( player ));
		}

		private bool IsValidVictim( Player player )
		{
			return (player.LifeState == LifeState.Alive && player.Team != Team);
		}

		private bool CanSeeTarget( Player player )
		{
			var muzzle = GetAttachment( "muzzle" );
			var trace = Trace.Ray( muzzle.Value.Position, player.WorldSpaceBounds.Center )
				.Ignore( this )
				.Size( 32f )
				.Run();

			return trace.Entity == player;
		}

		private bool IsFacingTarget()
		{
			var goalDirection = (Target.WorldSpaceBounds.Center - Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > (1f / RotateSpeed) )
				return false;

			return true;
		}
	}
}
