using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public interface ITurretComponent
	{
		public Vector3 GetTargetPosition( HoverPlayer target );
		public void FireProjectile( HoverPlayer target, Vector3 direction );
		public bool IsValidVictim( HoverPlayer target );
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
		[Net] public HoverPlayer Target { get; set; }

		public RealTimeUntil NextFindTarget { get; set; }
		public RealTimeUntil NextFireTime { get; set; }

		protected Vector3 ClientDirection { get; set; }

		protected virtual bool CanSeeTarget( HoverPlayer player )
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

		protected virtual bool IsValidTarget( HoverPlayer player )
		{
			if ( Entity is not ITurretComponent turret )
				return false;

			return (turret.IsValidVictim( player ) && CanSeeTarget( player ));
		}

		[GameEvent.Tick.Client]
		protected virtual void ClientTick()
		{
			UpdateAnimation();
		}

		protected virtual void UpdateAnimation()
		{
			if ( Entity is not ITurretComponent turret )
				return;

			ClientDirection = ClientDirection.LerpTo( TargetDirection, Time.Delta * turret.RotateSpeed );

			Entity.SetAnimParameter( "fire", Recoil );
			Entity.SetAnimParameter( "target", Entity.Transform.NormalToLocal( ClientDirection ) );
			Entity.SetAnimParameter( "weight", 1f );
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

		[GameEvent.Tick.Server]
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

			var targets = Sandbox.Entity.FindInSphere( Entity.Position, turret.AttackRadius )
				.OfType<HoverPlayer>()
				.Where( IsValidTarget );

			var closestTarget = (HoverPlayer)null;
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
