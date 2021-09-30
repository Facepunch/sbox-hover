using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class Player : Sandbox.Player
	{
		[Net, Predicted] public float Energy { get; set; }
		[Net, Local] public int Tokens { get; set; }
		[Net] public int KillStreak { get; set; }
		[Net] public float MaxHealth { get; set; }
		[Net] public float MaxEnergy { get; set; }
		[Net] public float MoveSpeed { get; set; }
		[Net] public float MaxSpeed { get; set; }

		public DamageInfo LastDamageInfo { get; set; }
		public Player LastKiller { get; set; }

		private Rotation LastCameraRotation { get; set; }
		private Radar RadarHud { get; set; }
		private float WalkBob { get; set; }
		private float FOV { get; set; }

		public bool HasTeam
		{
			get => Team != Team.None;
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

		public void Reset()
		{
			LastDamageInfo = default;
			LastKiller = null;
			Tokens = 0;
			Team = Team.None;
		}

		public void GiveTokens( int tokens )
		{
			Tokens += tokens;
		}

		public void TakeTokens( int tokens )
		{
			Tokens = Math.Max( Tokens - tokens, 0 );
		}

		public bool HasTokens( int tokens )
		{
			return (Tokens >= tokens);
		}

		public void GiveAward<T>() where T : Award
		{
			var award = Awards.Get<T>();

			if ( award != null )
			{
				var type = typeof( T ).Name;

				if ( award.TeamReward )
				{
					foreach ( var member in Team.GetAll() )
					{
						member.GiveTokens( award.Tokens );
						member.ShowAward( type );
					}
				}
				else
				{
					GiveTokens( award.Tokens );
					ShowAward( type );
				}
			}
		}

		[ClientRpc]
		public void ShowAward( string name )
		{
			var award = Awards.Get( name );

			if ( award != null )
			{
				Log.Info( Name + " earned the " + award.Name + " award and received " + award.Tokens + " token(s)!" );
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

		public override void ClientSpawn()
		{
			RadarHud = Local.Hud.AddChild<Radar>();

			base.ClientSpawn();
		}

		public override void Respawn()
		{
			Rounds.Current?.OnPlayerSpawn( this );

			RemoveRagdollEntity();

			base.Respawn();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			var attacker = LastAttacker as Player;

			if ( attacker.IsValid() )
			{
				if ( attacker.LifeState == LifeState.Alive )
					attacker.KillStreak++;
			}

			BecomeRagdollOnServer( LastDamageInfo.Force, GetHitboxBone( LastDamageInfo.HitboxIndex ) );
			Inventory.DeleteContents();

			KillStreak = 0;
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

			var forwardSpeed = Velocity.Normal.Dot( setup.Rotation.Forward );
			var speed = Velocity.Length.LerpInverse( 0, MaxSpeed );
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

				attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, ((float)Health).LerpInverse( 100, 0 ) );
			}

			TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position, info.Flags );

			if ( info.Flags.HasFlag( DamageFlags.Fall ) )
			{
				PlaySound( "fall" );
			}
			else if ( info.Flags.HasFlag( DamageFlags.Bullet ) )
			{
				PlaySound( "grunt" + Rand.Int( 1, 4 ) );
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

			if ( IsLocalPawn )
				RadarHud?.Delete();

			base.OnDestroy();
		}
	}
}
