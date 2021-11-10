﻿using Gamelib.Maths;
using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class BulletDropProjectile : ModelEntity
	{
		// TODO: Find a better way to achieve this without networking all these strings. Use a projectile data class?
		[Net, Predicted] public string ExplosionEffect { get; set; } = "";
		[Net, Predicted] public string LaunchSoundName { get; set; } = null;
		[Net, Predicted] public string FollowEffect { get; set; } = "";
		[Net, Predicted] public string TrailEffect { get; set; } = "";
		[Net, Predicted] public string HitSound { get; set; } = "";

		public Action<BulletDropProjectile, Entity> Callback { get; private set; }
		public List<string> FlybySounds { get; set; }
		public bool PlayFlybySounds { get; set; } = false;
		public RealTimeUntil CanHitTime { get; set; } = 0.1f;
		public ProjectileSimulator Simulator { get; set; }
		public float? LifeTime { get; set; }
		public string Attachment { get; set; } = null;
		public Entity Attacker { get; set; } = null;
		public ModelEntity Target { get; set; } = null;
		public float MoveTowardTarget { get; set; } = 0f;
		public bool ExplodeOnDestroy { get; set; } = true;
		public Entity IgnoreEntity { get; set; }
		public float Gravity { get; set; } = 10f;
		public float Radius { get; set; } = 8f;
		public bool FaceDirection { get; set; } = false;
		public Vector3 StartPosition { get; private set; }
		public bool Debug { get; set; } = false;

		private float GravityModifier { get; set; }
		private RealTimeUntil NextFlyby { get; set; }
		private RealTimeUntil DestroyTime { get; set; }
		private Sound LaunchSound { get; set; }
		private Particles Follower { get; set; }
		private Particles Trail { get; set; }

        public void Initialize( Vector3 start, Vector3 velocity, float radius, Action<BulletDropProjectile, Entity> callback = null )
		{
			Initialize( start, velocity, callback );
			Radius = radius;
		}

		public void Initialize( Vector3 start, Vector3 velocity, Action<BulletDropProjectile, Entity> callback = null )
		{
			if ( LifeTime.HasValue )
			{
				DestroyTime = LifeTime.Value;
			}

			if ( Simulator != null && Simulator.IsValid() )
            {
				Simulator?.Add( this );
				Owner = Simulator.Owner;
			}

			PhysicsEnabled = false;
			StartPosition = start;
			Velocity = velocity;
			Callback = callback;
			NextFlyby = 0.2f;
			Position = start;

			if ( IsClientOnly )
			{
				using ( Prediction.Off() )
				{
					CreateEffects();
				}
			}
		}

		public override void Spawn()
		{
			Predictable = true;

			base.Spawn();
		}

        public override void ClientSpawn()
        {
			if ( HasClientProxy() )
            {
				EnableDrawing = false;
				return;
            }

			CreateEffects();

            base.ClientSpawn();
        }

		public virtual void CreateEffects()
        {
			if ( !string.IsNullOrEmpty( TrailEffect ) )
			{
				Trail = Particles.Create( TrailEffect, this );

				if ( !string.IsNullOrEmpty( Attachment ) )
					Trail.SetEntityAttachment( 0, this, Attachment );
				else
					Trail.SetEntity( 0, this );
			}

			if ( !string.IsNullOrEmpty( FollowEffect ) )
			{
				Follower = Particles.Create( FollowEffect, this );
			}

			if ( !string.IsNullOrEmpty( LaunchSoundName ) )
				LaunchSound = PlaySound( LaunchSoundName );
		}

        public virtual void Simulate()
        {
			if ( FaceDirection )
            {
				Rotation = Rotation.LookAt( Velocity.Normal );
            }

			if ( Debug )
            {
				DebugOverlay.Sphere( Position, Radius, IsClient ? Color.Blue : Color.Red );
            }

			var newPosition = GetTargetPosition();

			var trace = Trace.Ray( Position, newPosition )
				.UseLagCompensation()
				.HitLayer( CollisionLayer.Water, true )
				.Size( Radius )
				.Ignore( this )
				.Ignore( IgnoreEntity )
				.Run();

			Position = trace.EndPos + trace.Direction.Normal * Radius;

			if ( LifeTime.HasValue && DestroyTime )
			{
				if ( ExplodeOnDestroy )
				{
					PlayHitEffects( Vector3.Zero );
					Callback?.Invoke( this, trace.Entity );
				}

				Delete();

				return;
			}

			if ( (trace.Hit && CanHitTime) || trace.StartedSolid )
			{
				PlayHitEffects( trace.Normal );
				Callback?.Invoke( this, trace.Entity );
				Delete();
			}
			else
			{
				if ( NextFlyby && FlybySounds != null )
				{
					WeaponUtil.PlayFlybySounds( Attacker, trace.StartPos, trace.EndPos, Radius, Radius * 4f, FlybySounds );
					NextFlyby = Time.Delta * 2f;
				}
			}
		}

		public bool HasClientProxy()
        {
			return !IsClientOnly && Owner.IsValid() && Owner.IsLocalPawn;

		}

		protected virtual Vector3 GetTargetPosition()
		{
			var newPosition = Position;
			newPosition += Velocity * Time.Delta;

			GravityModifier += Gravity;
			newPosition -= new Vector3( 0f, 0f, GravityModifier * Time.Delta );

			if ( Target.IsValid() && MoveTowardTarget > 0f )
			{
				var targetDirection = (Target.WorldSpaceBounds.Center - newPosition).Normal;
				newPosition += targetDirection * MoveTowardTarget * Time.Delta;
			}

			return newPosition;
		} 

		[ClientRpc]
		protected virtual void PlayHitEffects( Vector3 normal )
        {
			if ( HasClientProxy() )
            {
				return;
            }

			if ( !string.IsNullOrEmpty( ExplosionEffect ) )
			{
				var explosion = Particles.Create( ExplosionEffect );

				if ( explosion != null )
				{
					explosion.SetPosition( 0, Position );
					explosion.SetForward( 0, normal );
				}
			}

			if ( !string.IsNullOrEmpty( HitSound ) )
				Audio.Play( HitSound, Position );
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !Simulator.IsValid() )
			{
				Simulate();
			}
		}

        protected override void OnDestroy()
		{
			Simulator?.Remove( this );

			RemoveEffects();

			base.OnDestroy();
		}

		private void RemoveEffects()
		{
			LaunchSound.Stop();
			Follower?.Destroy();
			Trail?.Destroy();
			Trail = null;
		}
	}
}
