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
		public float RotateSpeed => 40f;
		public float AttackRadius => 3000f;
		public float FireRate => 1.5f;

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
			{
				Target = closestTarget;
			}
		}

		[Event.Tick.Server]
		private void UpdateTarget()
		{
			if ( NextFindTarget )
			{
				FindClosestTarget();
				NextFindTarget = 1f;
			}

			if ( Target.IsValid() )
			{
				TargetDirection = TargetDirection.LerpTo( (Target.Position - Position).Normal, Time.Delta * RotateSpeed );
				SetAnimVector( "target", Transform.NormalToLocal( TargetDirection ) );

				if ( NextFireTime && IsFacingTarget() )
				{
					FireProjectile();
					NextFireTime = FireRate;
				}
			}

			SetAnimFloat( "fire", Recoil );
			Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
		}

		private void FireProjectile()
		{
			if ( !Target.IsValid() ) return;

			Particles.Create( MuzzleFlash, this, "muzzle" );

			var projectile = new Projectile()
			{
				BezierCurve = false,
				Debug = true
			};

			var muzzle = GetAttachment( "muzzle" );

			projectile.Initialize( muzzle.Value.Position, Target.WorldSpaceBounds.Center, 0.3f );

			Recoil = 1f;
		}

		private bool IsValidTarget( Player player )
		{
			return (player.LifeState == LifeState.Alive && player.Team != Team);
		}

		private bool IsFacingTarget()
		{
			var goalDirection = (Target.Position - Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > (1f / RotateSpeed) )
				return false;

			return true;
		}
	}
}
