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
	public partial class TurretEntity : GeneratorDependency, IKillFeedIcon, ITurretComponent, IBaseAsset
	{
		public override List<DependencyUpgrade> Upgrades => new()
		{
			new TurretRangeUpgrade(),
			new TurretDamageUpgrade(),
			new TurretTargetingUpgrade()
		};

		public List<string> FlybySounds => new()
		{
			"flyby.rifleclose1",
			"flyby.rifleclose2",
			"flyby.rifleclose3",
			"flyby.rifleclose4"
		};

		public string MuzzleAttachment => "muzzle";
		public string MuzzleFlashEffect => "particles/weapons/muzzle_flash_plasma/muzzle_large/muzzleflash_large.vpcf";

		[Net] public float RotateSpeed { get; set; }
		public float ProjectileSpeed => 2500f;
		public float TargetingSpeed { get; set; }
		public float BlastDamage { get; set; }
		public float BlastRadius { get; set; }
		public float AttackRadius { get; set; }
		public float FireRate { get; set; }

		public string GetKillFeedIcon()
		{
			return "ui/killicons/turret.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Base Turret";
		}

		public void FireProjectile( Player target, Vector3 direction )
		{
			var projectile = new BulletDropProjectile()
			{
				FollowEffect = "particles/weapons/projectile_plasma.vpcf",
				TrailEffect = "particles/weapons/muzzle_flash_plasma/trail_effect.vpcf",
				ExplosionEffect = "particles/weapons/projectile_plasma_impact.vpcf",
				FlybySounds = FlybySounds,
				IgnoreEntity = this,
				LaunchSoundName = $"pulserifle.fire{Rand.Int( 1, 2 )}",
				MoveTowardTarget = 2000f,
				HitSound = "barage.explode",
				LifeTime = 10f,
				Target = target,
				Gravity = 0f
			};

			var muzzle = GetAttachment( "muzzle" );
			projectile.Initialize( muzzle.Value.Position, direction * ProjectileSpeed, 32f, OnProjectileHit );
		}

		public Vector3 GetTargetPosition( Player target )
		{
			var muzzle = GetAttachment( "muzzle" );
			var position = target.WorldSpaceBounds.Center;
			var timeToReach = (muzzle.Value.Position.Distance( position ) / ProjectileSpeed);
			return (position + target.Velocity * timeToReach);
		}

		public bool IsTurretDisabled()
		{
			return Team == Team.None;
		}

		public bool IsValidVictim( Player player )
		{
			if ( player.LifeState == LifeState.Dead )
				return false;

			if ( player.Team == Team )
				return false;

			if ( player.TargetAlpha == 0f )
				return false;

			return true;
		}

		public override void Spawn()
		{
			SetModel( "models/tempmodels/turret/turret.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Components.Create<TurretComponent>();

			base.Spawn();
		}

		public override void OnGameReset()
		{
			base.OnGameReset();

			TargetingSpeed = 1f;
			AttackRadius = 3000f;
			RotateSpeed = 10f;
			BlastDamage = 500f;
			BlastRadius = 300f;
			FireRate = 3f;
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
					.WithForce( (blastPosition - position).Normal * projectile.Velocity.Length * 0.1f )
					.WithPosition( blastPosition )
					.WithWeapon( this );

				damageInfo.Damage = BlastDamage - ((BlastDamage / BlastRadius) * distance);

				target.TakeDamage( damageInfo );
			}
		}
	}
}
