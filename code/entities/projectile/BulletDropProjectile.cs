using Gamelib.Maths;
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
		public Entity Attacker { get; set; } = null;
		public ModelEntity Target { get; set; } = null;
		public float MoveTowardTarget { get; set; } = 0f;
		public Entity IgnoreEntity { get; set; }
		public string HitSound { get; set; } = "";
		public float Gravity { get; set; } = 10f;
		public float Radius { get; set; } = 8f;
		public bool FaceDirection { get; set; } = false;
		public Vector3 StartPosition { get; private set; }
		public bool Debug { get; set; } = false;

		[Net, Predicted] public int ProjectileIndex { get; set; }

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

			if ( Attacker.IsValid() && Attacker is Player owner )
            {
				ProjectileIndex = owner.CurrentProjectileIndex;
				owner.CurrentProjectileIndex++;
				owner.Projectiles.Add( this );
				Owner = owner;
			}

			PhysicsEnabled = false;
			StartPosition = start;
			Velocity = velocity;
			Callback = callback;
			NextFlyby = 0.2f;
			Position = start;

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

		public override void Spawn()
		{
			Predictable = true;

			base.Spawn();
		}

        public override void ClientSpawn()
        {
			if ( !IsClientOnly && Owner.IsValid() && Owner.IsLocalPawn && Owner is Player owner )
			{
				if ( Local.Pawn is Player player )
				{
					var clientEntity = player.Projectiles.Find( v => v.ProjectileIndex == ProjectileIndex && v.IsClientOnly );

					if ( clientEntity.IsValid() )
					{
						// We have reconciled the entities. This is temporary.
					}
				}
			}

            base.ClientSpawn();
        }

		[Event.Tick.Client]
		private void HideServerEntity()
		{
			if ( !IsClientOnly && Owner.IsValid() && Owner.IsLocalPawn )
			{
				// This is a super hack for now. What we'll do is have the server and client entities merge together
				// or we'll simply not transmit it to the owner.
				Position = Vector3.Zero;
			}
		}

        public override void Simulate( Client client )
        {
			if ( !Prediction.FirstTime ) return;

			if ( FaceDirection )
				Rotation = Rotation.LookAt( Velocity.Normal );

			if ( Debug )
				DebugOverlay.Sphere( Position, Radius, Color.Red );

			if ( LifeTime.HasValue && DestroyTime )
			{
				Delete();
				return;
			}

			var newPosition = Position;
			newPosition += Velocity * Time.Delta;

			GravityModifier += Gravity;
			newPosition -= new Vector3( 0f, 0f, GravityModifier * Time.Delta );

			if ( Target.IsValid() && MoveTowardTarget > 0f )
			{
				var targetDirection = (Target.WorldSpaceBounds.Center - newPosition).Normal;
				newPosition += targetDirection * MoveTowardTarget * Time.Delta;
			}

			var trace = Trace.Ray( Position, newPosition )
				.UseLagCompensation()
				.HitLayer( CollisionLayer.Water, true )
				.Size( Radius )
				.Ignore( this )
				.Ignore( IgnoreEntity )
				.Run();

			Position = trace.EndPos + trace.Direction.Normal * Radius;

			if ( (trace.Hit && CanHitTime) || trace.StartedSolid )
			{
				if ( !string.IsNullOrEmpty( ExplosionEffect ) )
				{
					var explosion = Particles.Create( ExplosionEffect );

					if ( explosion != null )
                    {
						explosion.SetPosition( 0, Position );
						explosion.SetForward( 0, trace.Normal );
					}
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

			base.Simulate( client );
        }

        protected override void OnDestroy()
		{
			RemoveEffects();

			if ( Owner is Player owner )
            {
				owner.Projectiles.Remove( this );
            }

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
