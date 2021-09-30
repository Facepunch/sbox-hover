using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_turret" )]
	[Hammer.EditorModel( "models/tempmodels/turret/turret.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Turret", "Hover", "Defines a point where a team's turret spawns" )]
	public partial class TurretEntity : AnimEntity
	{
		[Property] public Team Team { get; set; }

		[Net] public Vector3 TargetDirection { get; private set; }
		[Net] public float Recoil { get; private set; }
		[Net] public Player Target { get; set; }

		public RealTimeUntil NextFindTarget { get; set; }
		public RealTimeUntil NextFireTime { get; set; }
		public string MuzzleFlash => "particles/weapons/muzzle_flash_plasma/muzzle_large/muzzleflash_large.vpcf";
		public float ProjectileSpeed => 10000f;
		public float RotateSpeed => 20f;
		public float AttackRadius => 3000f;
		public float FireRate => 2f;

		public override void Spawn()
		{
			SetModel( "models/tempmodels/turret/turret.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

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

			if ( closestTarget.IsValid() )
				Target = closestTarget;
			else
				Target = null;
		}

		[Event.Tick.Server]
		private void UpdateTarget()
		{
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
				Target = null;
			}

			SetAnimFloat( "fire", Recoil );
			Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
		}

		private void FireProjectile()
		{
			if ( !Target.IsValid() ) return;

			Particles.Create( MuzzleFlash, this, "muzzle" );

			var projectile = new PhysicsProjectile()
			{
				Debug = true
			};

			var muzzle = GetAttachment( "muzzle" );
			projectile.Initialize( muzzle.Value.Position, TargetDirection, 16f, ProjectileSpeed );

			Recoil = 1f;
		}

		private bool IsValidTarget( Player player )
		{
			return (player.LifeState == LifeState.Alive && player.Team != Team && CanSeeTarget( player ));
		}

		private bool CanSeeTarget( Player player )
		{
			var muzzle = GetAttachment( "muzzle" );
			var trace = Trace.Ray( muzzle.Value.Position, player.WorldSpaceBounds.Center )
				.Ignore( this )
				.Size( 16f )
				.Run();

			return trace.Entity == player;
		}

		private Vector3 GetProjectedPosition( Player target )
		{
			var position = target.WorldSpaceBounds.Center;
			var timeToReach = (Position.Distance( position ) / ProjectileSpeed);
			return (position + target.Velocity * timeToReach);
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
