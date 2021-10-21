using Gamelib.Maths;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class PhysicsProjectile : ModelEntity
	{
		public Action<PhysicsProjectile> Callback { get; private set; }
		public string ExplosionEffect { get; set; } = "";
		public float LifeTime { get; set; } = 5f;
		public string TrailEffect { get; set; } = "";
		public string LaunchSoundName { get; set; } = null;
		public string HitSound { get; set; } = "";

		protected RealTimeUntil DestroyTime { get; set; }
		protected Sound LaunchSound { get; set; }
		protected Particles Trail { get; set; }

		public void Initialize( Action<PhysicsProjectile> callback = null )
		{
			DestroyTime = LifeTime;
			Callback = callback;
			Transmit = TransmitType.Always;

			if ( !string.IsNullOrEmpty( TrailEffect ) )
			{
				Trail = Particles.Create( TrailEffect, this );
			}

			if ( !string.IsNullOrEmpty( LaunchSoundName ) )
				LaunchSound = PlaySound( LaunchSoundName );

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			OnInitialize();
		}

		public void Kill()
		{
			if ( !string.IsNullOrEmpty( ExplosionEffect ) )
			{
				var explosion = Particles.Create( ExplosionEffect );
				explosion.SetPosition( 0, Position );
			}

			if ( !string.IsNullOrEmpty( HitSound ) )
				Audio.Play( HitSound, Position );

			Callback?.Invoke( this );
			RemoveEffects();
			Delete();
		}

		protected virtual void OnInitialize() { }

		protected override void OnDestroy()
		{
			RemoveEffects();

			base.OnDestroy();
		}

		private void RemoveEffects()
		{
			LaunchSound.Stop();
			Trail?.Destroy();
			Trail = null;
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( DestroyTime )
			{
				Kill();
			}
		}
	}
}
