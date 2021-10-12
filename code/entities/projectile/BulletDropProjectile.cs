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
		public Action<BulletDropProjectile, Entity> Callback { get; private set; }
		public List<string> FlybySounds { get; set; }
		public bool PlayFlybySounds { get; set; } = false;
		public string ExplosionEffect { get; set; } = "";
		public RealTimeUntil CanHitTime { get; set; } = 0.1f;
		public float? LifeTime { get; set; }
		public string FollowEffect { get; set; } = "";
		public string TrailEffect { get; set; } = "";
		public string LaunchSoundName { get; set; } = null;
		public string Attachment { get; set; } = null;
		public Entity Attacker{ get; set; } = null;
		public ModelEntity Target { get; set; } = null;
		public float MoveTowardTarget { get; set; } = 0f;
		public Entity IgnoreEntity { get; set; }
		public string HitSound { get; set; } = "";
		public float Gravity { get; set; } = 300f;
		public float Radius { get; set; } = 16f;
		public float Speed { get; set; } = 2000f;
		public bool FaceDirection { get; set; } = false;
		public Vector3 StartPosition { get; private set; }
		public Vector3 Direction { get; set; }
		public bool Debug { get; set; } = false;

		private RealTimeUntil NextFlyby { get; set; }
		private RealTimeUntil DestroyTime { get; set; }
		private Sound LaunchSound { get; set; }
		private Particles Follower { get; set; }
		private Particles Trail { get; set; }

		public void Initialize( Vector3 start, Vector3 direction, float radius, float speed, Action<BulletDropProjectile, Entity> callback = null )
		{
			Initialize( start, direction, speed, callback );
			Radius = radius;
		}

		public void Initialize( Vector3 start, Vector3 direction, float speed, Action<BulletDropProjectile, Entity> callback = null )
		{
			if ( LifeTime.HasValue )
			{
				DestroyTime = LifeTime.Value;
			}

			PhysicsEnabled = false;
			StartPosition = start;
			Direction = direction;
			Callback = callback;
			NextFlyby = 0.2f;
			Position = start;
			Transmit = TransmitType.Always;
			Speed = speed;

			using ( Prediction.Off() )
			{
				if ( !string.IsNullOrEmpty( TrailEffect ) )
				{
					Trail = Particles.Create( TrailEffect );

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
		}

		protected override void OnDestroy()
		{
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

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( FaceDirection )
				Rotation = Rotation.LookAt( Direction );

			if ( Debug )
				DebugOverlay.Sphere( Position, Radius, Color.Red );

			if ( LifeTime.HasValue && DestroyTime )
			{
				Delete();
				return;
			}

			var newPosition = Position;
			newPosition += Direction * Speed * Time.Delta;
			newPosition -= new Vector3( 0f, 0f, Gravity * Time.Delta );

			if ( Target.IsValid() && MoveTowardTarget > 0f )
			{
				var targetDirection = (Target.WorldSpaceBounds.Center - newPosition).Normal;
				newPosition += targetDirection * MoveTowardTarget * Time.Delta;
			}

			var trace = Trace.Ray( Position, newPosition )
				.Size( Radius )
				.Ignore( this )
				.Ignore( IgnoreEntity )
				.Run();

			Position = trace.EndPos + Direction * Radius;

			if ( (trace.Hit && CanHitTime) || trace.StartedSolid )
			{
				if ( !string.IsNullOrEmpty( ExplosionEffect ) )
				{
					var explosion = Particles.Create( ExplosionEffect );
					explosion.SetPosition( 0, Position );
					explosion.SetForward( 0, trace.Normal );
				}

				if ( !string.IsNullOrEmpty( HitSound ) )
					Audio.Play( HitSound, Position );

				Callback?.Invoke( this, trace.Entity );
				RemoveEffects();
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
	}
}
