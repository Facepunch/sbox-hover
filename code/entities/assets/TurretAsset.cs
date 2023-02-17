using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Gamelib.Utility;
using Editor;

namespace Facepunch.Hover
{
	[Library( "hv_turret" )]
	[EditorModel( "models/tempmodels/turret/turret.vmdl", FixedBounds = true )]
	[Title( "Turret" )]
	[Sphere( 3000, 75, 255, 65)]
	[HammerEntity]
	public partial class TurretAsset : GeneratorDependency, IKillFeedIcon, ITurretComponent, IBaseAsset
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

		[BindComponent] public TurretComponent TurretComponent { get; }

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

		public void FireProjectile( HoverPlayer target, Vector3 direction )
		{
			var projectile = Projectile.Create<TurretProjectile>( "turret" );

			projectile.FlybySounds = FlybySounds;
			projectile.IgnoreEntity = this;
			projectile.MoveTowardTarget = 2000f;
			projectile.Target = target;

			var muzzle = GetAttachment( "muzzle" );
			projectile.Initialize( muzzle.Value.Position, direction * ProjectileSpeed, OnProjectileHit );
		}

		public Vector3 GetTargetPosition( HoverPlayer target )
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

		public bool IsValidVictim( HoverPlayer player )
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
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

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

		private void OnProjectileHit( Projectile projectile, Entity victim )
		{
			var blastPosition = projectile.Position;

			var proximity = WeaponUtil.GetBlastEntities<HoverPlayer>( blastPosition, BlastRadius )
				.Where( IsValidVictim );

			foreach ( var target in proximity )
			{
				var position = target.Position;
				var distance = position.Distance( blastPosition );
				var damageInfo = new DamageInfo()
					.WithAttacker( this )
					.WithTag( "blast" )
					.WithForce( (blastPosition - position).Normal * projectile.Velocity.Length * 0.1f )
					.WithPosition( blastPosition )
					.WithWeapon( this );

				damageInfo.Damage = BlastDamage - ((BlastDamage / BlastRadius) * distance);

				target.TakeDamage( damageInfo );
			}
		}
	}
}
