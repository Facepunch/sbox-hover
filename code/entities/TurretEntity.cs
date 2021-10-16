using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_turret" )]
	[Hammer.EditorModel( "models/tempmodels/turret/turret.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Turret", "Hover", "Defines a point where a turret spawns" )]
	[Hammer.Sphere( 3000, 75, 255, 65)]
	public partial class TurretEntity : GeneratorDependency
	{
		public override List<DependencyUpgrade> Upgrades => new()
		{
			new TurretRangeUpgrade(),
			new TurretDamageUpgrade(),
			new TurretRangeUpgrade()
		};

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

		public RealTimeUntil NextFindTarget { get; set; }
		public RealTimeUntil NextFireTime { get; set; }
		public string MuzzleFlash => "particles/weapons/muzzle_flash_plasma/muzzle_large/muzzleflash_large.vpcf";
		public float ProjectileSpeed => 4000f;
		public float RotateSpeed => 10f;
		public float AttackRadius { get; set; }
		public float BlastDamage { get; set; }
		public float BlastRadius { get; set; }
		public float FireRate => 3f;

		private Vector3 ClientDirection { get; set; }

		public override void OnGameReset()
		{
			AttackRadius = 3000f;
			BlastDamage = 500f;
			BlastRadius = 300f;

			base.OnGameReset();
		}

		public override void Spawn()
		{
			SetModel( "models/tempmodels/turret/turret.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

			Name = "Turret";

			base.Spawn();
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
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
				NextFireTime = 1f;
			}

			if ( closestTarget.IsValid() )
				Target = closestTarget;
			else
				Target = null;
		}

		[Event.Tick.Server]
		private void UpdateTarget()
		{
			if ( IsPowered )
			{
				if ( NextFindTarget )
				{
					FindClosestTarget();
					NextFindTarget = 0.1f;
				}

				if ( Target.IsValid() && IsValidTarget( Target ) )
				{
					TargetDirection = (GetProjectedPosition( Target ) - Position).Normal;

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

		[Event.Tick.Client]
		private void ClientTick()
		{
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

			Particles.Create( MuzzleFlash, this, "muzzle" );

			var projectile = new BulletDropProjectile()
			{
				FollowEffect = "particles/weapons/projectile_plasma.vpcf",
				TrailEffect = "particles/weapons/muzzle_flash_plasma/trail_effect.vpcf",
				ExplosionEffect = "particles/weapons/projectile_plasma_impact.vpcf",
				FlybySounds = FlybySounds,
				IgnoreEntity = this,
				LaunchSoundName = $"pulserifle.fire{Rand.Int(1, 2)}",
				MoveTowardTarget = 500f,
				HitSound = "barage.explode",
				LifeTime = 10f,
				Target = Target,
				Gravity = 100f
			};

			var muzzle = GetAttachment( "muzzle" );
			projectile.Initialize( muzzle.Value.Position, TargetDirection, 32f, ProjectileSpeed, OnProjectileHit );

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

		private Vector3 GetProjectedPosition( Player target )
		{
			var muzzle = GetAttachment( "muzzle" );
			var position = target.WorldSpaceBounds.Center;
			var timeToReach = (muzzle.Value.Position.Distance( position ) / ProjectileSpeed);
			return (position + target.Velocity * timeToReach);
		}

		private void OnProjectileHit( BulletDropProjectile projectile, Entity victim )
		{
			var blastPosition = projectile.Position;

			var proximity = Physics.GetEntitiesInSphere( blastPosition, BlastRadius )
				.OfType<Player>()
				.Where( IsValidVictim );

			foreach ( var target in proximity )
			{
				var position = target.Position;
				var distance = position.Distance( blastPosition );
				var damageInfo = new DamageInfo()
					.WithAttacker( this )
					.WithFlag( DamageFlags.Blast | DamageFlags.Shock )
					.WithForce( (blastPosition - position).Normal * projectile.Speed * 0.1f )
					.WithPosition( blastPosition )
					.WithWeapon( this );

				damageInfo.Damage = BlastDamage - ((BlastDamage / BlastRadius) * distance);

				target.TakeDamage( damageInfo );
			}
		}

		private bool IsFacingTarget()
		{
			var goalDirection = (GetProjectedPosition( Target )- Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > (1f / RotateSpeed) )
				return false;

			return true;
		}
	}
}
