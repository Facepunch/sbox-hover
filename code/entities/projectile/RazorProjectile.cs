using Gamelib.Maths;
using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public partial class RazorProjectile : Projectile
	{
		[Net, Predicted] public bool HasTarget { get; set; }
		[Net, Predicted] public HoverPlayer Target { get; set; }

		public Vector3 UpVelocity { get; set; }
		public float SeekRadius { get; set; }

		private Color TrailColor { get; set; }

		public override void CreateEffects()
		{
			TrailColor = Color.Green;

			if ( !string.IsNullOrEmpty( Data.TrailEffect ) )
			{
				Trail = Particles.Create( Data.TrailEffect, this );

				if ( !string.IsNullOrEmpty( Attachment ) )
					Trail.SetEntityAttachment( 0, this, Attachment );
				else
					Trail.SetEntity( 0, this );

				Trail.SetPosition( 1, TrailColor * 255f );
			}

			if ( !string.IsNullOrEmpty( Data.FollowEffect ) )
			{
				Follower = Particles.Create( Data.FollowEffect, this );
				Follower.SetPosition( 1, TrailColor * 255f );
			}

			if ( !string.IsNullOrEmpty( Data.LaunchSound ) )
			{
				LaunchSound = PlaySound( Data.LaunchSound );
			}
		}

		protected override void PreRender()
		{
			if ( Target.IsValid() )
				TrailColor = Color.Lerp( TrailColor, Color.Red, Time.Delta * 10f );
			else
				TrailColor = Color.Lerp( TrailColor, Color.Green, Time.Delta * 10f );

			Trail?.SetPosition( 1, TrailColor * 255f );

			base.PreRender();
		}

		protected override Vector3 GetTargetPosition()
		{
			var newPosition = base.GetTargetPosition();

			var targets = FindInSphere( Position, SeekRadius )
				.OfType<HoverPlayer>()
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
			if ( IsServerSideCopy() )
			{
				return;
			}

			if ( !string.IsNullOrEmpty( Data.ExplosionEffect ) )
			{
				var explosion = Particles.Create( Data.ExplosionEffect );

				if ( explosion != null )
				{
					explosion.SetPosition( 0, Position );
					explosion.SetForward( 0, normal );
					explosion.SetPosition( 1, TrailColor * 255f );
				}
			}

			if ( !string.IsNullOrEmpty( Data.HitSound ) )
			{
				Audio.Play( Data.HitSound, Position );
			}
		}

		private bool IsValidTarget( HoverPlayer player )
		{
			if ( player.LifeState == LifeState.Dead )
			{
				return false;
			}

			if ( Attacker is HoverPlayer attacker )
			{
				return attacker.Team != player.Team;
			}

			return true;
		}
	}
}
