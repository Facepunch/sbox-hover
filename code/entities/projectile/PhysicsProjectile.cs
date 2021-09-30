using Gamelib.Maths;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class PhysicsProjectile : ModelEntity
	{
		private Particles Trail { get; set; }
		public Action<PhysicsProjectile, Entity> Callback { get; private set; }
		public string ExplosionEffect { get; set; } = "";
		public RealTimeUntil CanHitTime { get; set; } = 0.1f;
		public RealTimeUntil LifeTime { get; set; } = 10f;
		public string TrailEffect { get; set; } = "";
		public string LaunchSound { get; set; } = null;
		public string Attachment { get; set; } = null;
		public string HitSound { get; set; } = "";
		public float Gravity { get; set; } = 300f;
		public float Radius { get; set; } = 16f;
		public float Speed { get; set; } = 2000f;
		public bool FaceDirection { get; set; } = false;
		public Vector3 Direction { get; set; }
		public bool Debug { get; set; } = false;

		private Sound _launchSound;

		public PhysicsProjectile()
		{
			Transmit = TransmitType.Always;
		}

		public void Initialize( Vector3 start, Vector3 direction, float radius, float speed, Action<PhysicsProjectile, Entity> callback = null )
		{
			Initialize( start, direction, speed, callback );
			Radius = radius;
		}

		public void Initialize( Vector3 start, Vector3 direction, float speed, Action<PhysicsProjectile, Entity> callback = null )
		{
			PhysicsEnabled = false;
			Direction = direction;
			Callback = callback;
			Position = start;
			Speed = speed;

			if ( !string.IsNullOrEmpty( TrailEffect ) )
			{
				Trail = Particles.Create( TrailEffect );

				if ( !string.IsNullOrEmpty( Attachment ) )
					Trail.SetEntityAttachment( 0, this, Attachment );
				else
					Trail.SetEntity( 0, this );
			}

			if ( !string.IsNullOrEmpty( LaunchSound ) )
				_launchSound = PlaySound( LaunchSound );
		}

		protected override void OnDestroy()
		{
			RemoveEffects();

			base.OnDestroy();
		}

		private void RemoveEffects()
		{
			_launchSound.Stop();
			Trail?.Destroy();
			Trail = null;
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( FaceDirection )
				Rotation = Rotation.LookAt( Velocity );

			if ( Debug )
				DebugOverlay.Sphere( Position, Radius, Color.Red );

			var newPosition = Position;
			newPosition += Direction * Speed * Time.Delta;
			newPosition -= new Vector3( 0f, 0f, Gravity * Time.Delta );

			var trace = Trace.Ray( Position, newPosition )
				.Size( Radius )
				.Ignore( this )
				.Run();

			Position = newPosition;

			if ( trace.Hit || LifeTime )
			{
				if ( !string.IsNullOrEmpty( ExplosionEffect ) )
				{
					var explosion = Particles.Create( ExplosionEffect );
					explosion.SetPosition( 0, Position );
				}

				Log.Info( trace.Entity );

				if ( !string.IsNullOrEmpty( HitSound ) )
					Audio.Play( HitSound, Position );

				Callback?.Invoke( this, trace.Entity );
				RemoveEffects();
				Delete();
			}
		}
	}
}
