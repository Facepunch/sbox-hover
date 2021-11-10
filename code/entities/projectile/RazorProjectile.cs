using Gamelib.Maths;
using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public partial class RazorProjectile : BulletDropProjectile
	{
		[Net, Predicted] public bool HasTarget { get; set; }
		[Net, Predicted] public Player Target { get; set; }

		public Vector3 UpVelocity { get; set; }
		public float SeekRadius { get; set; }

		private Color TrailColor { get; set; }

		public override void CreateEffects()
		{
			TrailColor = Color.Green;

			if ( !string.IsNullOrEmpty( TrailEffect ) )
			{
				Trail = Particles.Create( TrailEffect, this );

				if ( !string.IsNullOrEmpty( Attachment ) )
					Trail.SetEntityAttachment( 0, this, Attachment );
				else
					Trail.SetEntity( 0, this );

				Trail.SetPosition( 1, TrailColor * 255f );
			}

			if ( !string.IsNullOrEmpty( FollowEffect ) )
			{
				Follower = Particles.Create( FollowEffect, this );
				Follower.SetPosition( 1, TrailColor * 255f );
			}

			if ( !string.IsNullOrEmpty( LaunchSoundName ) )
				LaunchSound = PlaySound( LaunchSoundName );
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( Target.IsValid() )
				TrailColor = Color.Lerp( TrailColor, Color.Red, Time.Delta * 10f );
			else
				TrailColor = Color.Lerp( TrailColor, Color.Green, Time.Delta * 10f );

			Trail?.SetPosition( 1, TrailColor * 255f );
		}

		protected override Vector3 GetTargetPosition()
		{
			var newPosition = base.GetTargetPosition();

			var targets = Physics.GetEntitiesInSphere( Position, SeekRadius )
				.OfType<Player>()
				.Where( IsValidTarget );

			var target = targets.FirstOrDefault();

			if ( target.IsValid() )
			{
				// Remove some of the velocity we added before and chase the target.
				newPosition -= Velocity * 0.75f * Time.Delta;
				var targetDirection = (target.WorldSpaceBounds.Center - newPosition).Normal;
				newPosition += targetDirection * Velocity.Length * 0.5f * Time.Delta;
			}
			else
			{
				// We have no target so move the projectile up if we should.
				newPosition += UpVelocity * Time.Delta;
				UpVelocity -= UpVelocity * 0.75f * Time.Delta;
			}

			Target = target;

			return newPosition;
		}

		[ClientRpc]
		protected override void PlayHitEffects( Vector3 normal )
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
					explosion.SetPosition( 1, TrailColor * 255f );
				}
			}

			if ( !string.IsNullOrEmpty( HitSound ) )
				Audio.Play( HitSound, Position );
		}

		private bool IsValidTarget( Player player )
		{
			if ( player.LifeState == LifeState.Dead )
			{
				return false;
			}

			if ( Attacker is Player attacker )
			{
				return attacker.Team != player.Team;
			}

			return true;
		}
	}
}
