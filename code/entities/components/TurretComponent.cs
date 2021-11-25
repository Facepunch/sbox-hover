using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public interface ITurretComponent
	{
		public Vector3 GetTargetPosition( Player target );
		public void FireProjectile( Player target, Vector3 direction );
		public bool IsValidVictim( Player target );
		public bool IsTurretDisabled();
		public string MuzzleAttachment { get; }
		public string MuzzleFlashEffect { get; }
		public float RotateSpeed { get; }
		public float TargetingSpeed { get; }
		public float AttackRadius { get; }
		public float FireRate { get; }
	}

	public partial class TurretComponent : EntityComponent<GeneratorDependency>
	{
		[Net] public Vector3 TargetDirection { get; private set; }
		[Net] public float Recoil { get; private set; }
		[Net] public Player Target { get; set; }

		public virtual List<string> FlybySounds => new()
		{
			"flyby.rifleclose1",
			"flyby.rifleclose2",
			"flyby.rifleclose3",
			"flyby.rifleclose4"
		};

		public RealTimeUntil NextFindTarget { get; set; }
		public RealTimeUntil NextFireTime { get; set; }

		protected Vector3 ClientDirection { get; set; }

		protected virtual bool CanSeeTarget( Player player )
		{
			if ( Entity is not ITurretComponent turret )
				return false;

			var muzzle = Entity.GetAttachment( turret.MuzzleAttachment );
			var trace = Trace.Ray( muzzle.Value.Position, player.WorldSpaceBounds.Center )
				.Ignore( Entity )
				.Size( 24f )
				.Run();

			return trace.Entity == player;
		}

		protected virtual bool IsValidTarget( Player player )
		{
			if ( Entity is not ITurretComponent turret )
				return false;

			return (turret.IsValidVictim( player ) && CanSeeTarget( player ));
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			UpdateAnimation();
		}

		protected virtual void UpdateAnimation()
		{
			if ( Entity is not ITurretComponent turret )
				return;

			ClientDirection = ClientDirection.LerpTo( TargetDirection, Time.Delta * turret.RotateSpeed );

			Entity.SetAnimFloat( "fire", Recoil );
			Entity.SetAnimVector( "target", Entity.Transform.NormalToLocal( ClientDirection ) );
			Entity.SetAnimFloat( "weight", 1f );
		}

		protected virtual void FireProjectile()
		{
			if ( Entity is not ITurretComponent turret )
				return;

			if ( !string.IsNullOrEmpty( turret.MuzzleFlashEffect ) && !string.IsNullOrEmpty( turret.MuzzleAttachment ) )
			{
				Particles.Create( turret.MuzzleFlashEffect, Entity, turret.MuzzleAttachment );
			}

			turret.FireProjectile( Target, TargetDirection );

			Recoil = 1f;
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( Entity is not ITurretComponent turret )
				return;

			if ( turret.IsTurretDisabled() )
				return;

			if ( Entity.IsPowered )
			{
				if ( NextFindTarget )
				{
					FindClosestTarget();
					NextFindTarget = 0.1f;
				}

				if ( Target.IsValid() && IsValidTarget( Target ) )
				{
					TargetDirection = (turret.GetTargetPosition( Target ) - Entity.Position).Normal;

					if ( NextFireTime && IsFacingTarget() )
					{
						FireProjectile();
						NextFireTime = turret.FireRate;
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
				var positionAhead = (Entity.Position + Entity.Rotation.Forward * 500f) + Vector3.Down * 200f;
				TargetDirection = (positionAhead - Entity.Position).Normal;
			}

			UpdateAnimation();

			Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
		}

		protected virtual bool IsFacingTarget()
		{
			if ( Entity is not ITurretComponent turret )
				return false;

			var goalDirection = (turret.GetTargetPosition( Target ) - Entity.Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > (1f / turret.RotateSpeed) )
				return false;

			return true;
		}

		protected virtual void FindClosestTarget()
		{
			if ( Entity is not ITurretComponent turret )
				return;

			var targets = Physics.GetEntitiesInSphere( Entity.Position, turret.AttackRadius )
				.OfType<Player>()
				.Where( IsValidTarget );

			var closestTarget = (Player)null;
			var closestDistance = 0f;

			foreach ( var target in targets )
			{
				var distance = target.Position.Distance( Entity.Position );

				if ( !closestTarget.IsValid() || distance < closestDistance )
				{
					closestTarget = target;
					closestDistance = distance;
				}
			}

			if ( closestTarget != Target && NextFireTime < turret.TargetingSpeed )
			{
				NextFireTime = turret.TargetingSpeed;
			}

			if ( closestTarget.IsValid() )
				Target = closestTarget;
			else
				Target = null;
		}
	}
}
