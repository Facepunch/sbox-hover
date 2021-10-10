using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_turret" )]
	[Hammer.EditorModel( "models/tempmodels/turret/turret.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Turret", "Hover", "Defines a point where a team's turret spawns" )]
	[Hammer.Sphere( 3000, 75, 255, 65)]
	public partial class TurretEntity : AnimEntity, IHudEntity, IGameResettable
	{
		[Property] public Team Team { get; set; }

		[Net] public Vector3 TargetDirection { get; private set; }
		[Net] public float Recoil { get; private set; }
		[Net] public Player Target { get; set; }
		[Net, Change] public bool IsPowered { get; set; } = true;

		public RealTimeUntil NextFindTarget { get; set; }
		public RealTimeUntil NextFireTime { get; set; }
		public string MuzzleFlash => "particles/weapons/muzzle_flash_plasma/muzzle_large/muzzleflash_large.vpcf";
		public float ProjectileSpeed => 4000f;
		public float RotateSpeed => 20f;
		public float AttackRadius => 3000f;
		public float BlastDamage => 400f;
		public float BlastRadius => 300f;
		public float FireRate => 2.2f;

		public EntityHudIcon PowerIcon { get; private set; }
		public EntityHudAnchor Hud { get; private set; }

		public Vector3 LocalCenter => CollisionBounds.Center;

		public void OnGameReset()
		{
			IsPowered = true;
		}

		public bool ShouldUpdateHud()
		{
			return true;
		}

		public void UpdateHudComponents()
		{
			var distance = Local.Pawn.Position.Distance( Position ) - 1000f;
			var mapped = 1f - distance.Remap( 0f, 1000f, 0f, 1f );

			if ( Hud.Style.Opacity != mapped )
			{
				Hud.Style.Opacity = mapped;
				Hud.Style.Dirty();
			}
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

			GeneratorEntity.OnGeneratorBroken += OnGeneratorBroken;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Hud = EntityHud.Instance.Create( this );

			PowerIcon = Hud.AddChild<EntityHudIcon>( "power" );
			PowerIcon.SetTexture( "ui/icons/power.png" );

			base.ClientSpawn();
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
		}

		protected override void OnDestroy()
		{
			GeneratorEntity.OnGeneratorBroken -= OnGeneratorBroken;

			base.OnDestroy();
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

			if ( closestTarget.IsValid() )
				Target = closestTarget;
			else
				Target = null;
		}

		[Event.Tick.Server]
		private void UpdateTarget()
		{
			if ( !IsPowered )
			{
				var positionAhead = (Position + Rotation.Forward * 500f) + Vector3.Down * 200f;
				TargetDirection = (positionAhead - Position).Normal;
				SetAnimVector( "target", Transform.NormalToLocal( TargetDirection ) );
				return;
			}

			if ( NextFindTarget )
			{
				FindClosestTarget();
				NextFindTarget = 0.1f;
			}

			if ( Target.IsValid() && IsValidTarget( Target ) )
			{
				TargetDirection = TargetDirection.LerpTo( (GetProjectedPosition( Target ) - Position).Normal, Time.Delta * RotateSpeed );
				SetAnimVector( "target", Transform.NormalToLocal( TargetDirection ) );

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

			SetAnimFloat( "fire", Recoil );
			Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
		}

		private void FireProjectile()
		{
			if ( !Target.IsValid() ) return;

			Particles.Create( MuzzleFlash, this, "muzzle" );

			var projectile = new BulletDropProjectile()
			{
				FollowEffect = "particles/weapons/projectile_plasma.vpcf",
				TrailEffect = "particles/weapons/muzzle_flash_plasma/trail_effect.vpcf",
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

		private void OnGeneratorBroken( GeneratorEntity generator )
		{
			if ( generator.Team == Team )
			{
				IsPowered = false;
			}
		}

		private void OnIsPoweredChanged( bool isPowered )
		{
			if ( isPowered )
				PowerIcon.SetTexture( "ui/icons/power.png" );
			else
				PowerIcon.SetTexture( "ui/icons/no-power.png" );
		}
	}
}
