using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class Player : Sandbox.Player
	{
		[Net, Predicted] public float Stamina { get; set; }

		private Rotation LastCameraRotation { get; set; }
		private DamageInfo LastDamageInfo { get; set; }
		private float WalkBob { get; set; }
		private float FOV { get; set; }

		public bool HasTeam
		{
			get => Team != null;
		}

		public Player()
		{
			Inventory = new Inventory( this );
			Animator = new StandardPlayerAnimator();
		}

		public bool IsSpectator
		{
			get => (Camera is SpectateCamera);
		}

		public Vector3 SpectatorDeathPosition
		{
			get
			{
				if ( Camera is SpectateCamera camera )
					return camera.DeathPosition;

				return Vector3.Zero;
			}
		}

		public bool HasSpectatorTarget
		{
			get
			{
				var target = SpectatorTarget;
				return (target != null && target.IsValid());
			}
		}

		public Player SpectatorTarget
		{
			get
			{
				if ( Camera is SpectateCamera camera )
					return camera.TargetPlayer;

				return null;
			}
		}

		public void MakeSpectator( Vector3 position = default )
		{
			EnableAllCollisions = false;
			EnableDrawing = false;
			Controller = null;
			Camera = new SpectateCamera
			{
				DeathPosition = position,
				TimeSinceDied = 0
			};
		}

		public override void Respawn()
		{
			Rounds.Current?.OnPlayerSpawn( this );

			RemoveRagdollEntity();

			Stamina = 100f;

			base.Respawn();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			BecomeRagdollOnServer( LastDamageInfo.Force, GetHitboxBone( LastDamageInfo.HitboxIndex ) );
			Inventory.DeleteContents();
			Team?.OnPlayerKilled( this );
		}

		public override void Simulate( Client client )
		{
			SimulateActiveChild( client, ActiveChild );

			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( LifeState != LifeState.Alive )
			{
				return;
			}

			TickPlayerUse();

			if ( Team != null )
			{
				Team.OnTick( this );
			}

			if ( ActiveChild is Weapon weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f && weapon.TimeSinceSecondaryAttack > 0.5f )
			{
				SwitchToBestWeapon();
			}

			var controller = GetActiveController();
			controller?.Simulate( client, this, GetActiveAnimator() );
		}

		protected override void UseFail()
		{
			// Do nothing. By default this plays a sound that we don't want.
		}

		public void SwitchToBestWeapon()
		{
			var best = Children.Select( x => x as Weapon )
				.Where( x => x.IsValid() && x.IsUsable() )
				.OrderByDescending( x => x.BucketWeight )
				.FirstOrDefault();

			if ( best == null ) return;

			ActiveChild = best;
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );

			if ( LastCameraRotation == Rotation.Identity )
				LastCameraRotation = CurrentView.Rotation;

			var angleDiff = Rotation.Difference( LastCameraRotation, CurrentView.Rotation );
			var angleDiffDegrees = angleDiff.Angle();
			var allowance = 20.0f;

			if ( angleDiffDegrees > allowance )
			{
				LastCameraRotation = Rotation.Lerp( LastCameraRotation, CurrentView.Rotation, 1.0f - (allowance / angleDiffDegrees) );
			}
			
			AddCameraEffects( ref setup );
		}

		private void AddCameraEffects( ref CameraSetup setup )
		{
			if ( Controller is not MoveController controller ) return;

			var speed = Velocity.Length.LerpInverse( 0, controller.MaxSpeed );
			var forwardSpeed = Velocity.Normal.Dot( setup.Rotation.Forward );

			var left = setup.Rotation.Left;
			var up = setup.Rotation.Up;

			if ( GroundEntity != null )
			{
				WalkBob += Time.Delta * 25.0f * speed;
			}

			setup.Position += up * MathF.Sin( WalkBob ) * speed * 2;
			setup.Position += left * MathF.Sin( WalkBob * 0.6f ) * speed * 1;

			speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

			FOV = FOV.LerpTo( speed * 30 * MathF.Abs( forwardSpeed ), Time.Delta * 2.0f );

			setup.FieldOfView += FOV;
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( info.HitboxIndex == 0 )
			{
				info.Damage *= 2.0f;
			}

			if ( info.Attacker is Player attacker && attacker != this )
			{
				if ( !Game.AllowFriendlyFire )
				{
					return;
				}

				Team?.OnTakeDamageFromPlayer( this, attacker, info );
				attacker.Team?.OnDealDamageToPlayer( attacker, this, info );

				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, ((float)Health).LerpInverse( 100, 0 ) );
			}

			TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position, info.Flags );

			if ( info.Flags.HasFlag( DamageFlags.Fall ) )
			{
				PlaySound( "fall" );
			}
			else if ( info.Flags.HasFlag( DamageFlags.Bullet ) )
			{
				if ( !Team?.PlayPainSounds( this ) == false )
				{
					PlaySound( "grunt" + Rand.Int( 1, 4 ) );
				}
			}

			LastDamageInfo = info;

			base.TakeDamage( info );
		}

		public void RemoveRagdollEntity()
		{
			if ( Ragdoll != null && Ragdoll.IsValid() )
			{
				Ragdoll.Delete();
				Ragdoll = null;
			}
		}

		[ClientRpc]
		public void DidDamage( Vector3 position, float amount, float inverseHealth )
		{
			Sound.FromScreen( "dm.ui_attacker" )
				.SetPitch( 1 + inverseHealth * 1 );

			HitIndicator.Current?.OnHit( position, amount );
		}

		[ClientRpc]
		public void TookDamage( Vector3 position, DamageFlags flags )
		{
			if ( flags.HasFlag(DamageFlags.Fall) )
				_ = new Sandbox.ScreenShake.Perlin(2f, 1f, 1.5f, 0.8f);

			DamageIndicator.Current?.OnHit( position );
		}

		protected override void OnDestroy()
		{
			RemoveRagdollEntity();

			base.OnDestroy();
		}
	}
}
