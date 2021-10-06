﻿using Sandbox;
using Sandbox.Joints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class Player : Sandbox.Player
	{
		private class AssistTracker
		{
			public TimeSince LastDamageTime { get; set; }
			public Player Attacker { get; set; }
			public float TotalDamage { get; set; }
		}

		[Net, Local] public int Tokens { get; set; }
		[Net] public float HealthRegen { get; set; }
		[Net] public float RegenDelay { get; set; }
		[Net] public RealTimeUntil NextRegenTime { get; set; }
		[Net] public int KillStreak { get; set; }
		[Net] public float MaxHealth { get; set; }

		public RealTimeUntil RespawnTime { get; set; }
		public DamageInfo LastDamageInfo { get; set; }
		public Player LastKiller { get; set; }

		private List<AssistTracker> AssistTrackers { get; set; }
		private Rotation LastCameraRotation { get; set; }
		private Particles SpeedLines { get; set; }
		private Radar RadarHud { get; set; }
		private bool PlayLowEnergySound { get; set; }
		private bool IsPlayingJetpackLoop { get; set; }
		private bool IsPlayingSkiLoop { get; set; }
		private bool IsRegenerating { get; set; }
		private Sound JetpackLoop { get; set; }
		private Sound SkiLoop { get; set; }
		private float WalkBob { get; set; }
		private float FOV { get; set; }

		public bool HasTeam
		{
			get => Team != Team.None;
		}

		public Player()
		{
			AssistTrackers = new();
			EnableTouch = true;
			Inventory = new Inventory( this );
			Animator = new StandardPlayerAnimator();
		}

		public bool IsSpectator
		{
			get => (Camera is SpectateCamera);
		}

		public void Reset()
		{
			Client.SetInt( "captures", 0 );
			Client.SetInt( "deaths", 0 );
			Client.SetInt( "kills", 0 );

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
						member.ShowAward( To.Single( member ), type );
					}
				}
				else
				{
					GiveTokens( award.Tokens );
					ShowAward( To.Single( this ), type );
				}
			}
		}

		public void ApplyForce( Vector3 force )
		{
			if ( Controller is MoveController controller )
			{
				controller.Impulse += force;
			}
		}

		[ClientRpc]
		public void ShowAward( string name )
		{
			var award = Awards.Get( name );

			if ( award != null )
			{
				award.Show();
			}
		}

		public Player GetBestAssist( Entity attacker )
		{
			var minDamageTarget = MaxHealth * 0.3f;
			var assister = (Player)null;
			var damage = 0f;

			foreach ( var tracker in AssistTrackers )
			{
				if ( tracker.Attacker != attacker && tracker.TotalDamage >= minDamageTarget )
				{
					if ( assister == null || tracker.TotalDamage > damage )
					{
						assister = tracker.Attacker;
						damage = tracker.TotalDamage;
					}
				}
			}

			return assister;
		}

		public void MakeSpectator( Vector3 position, float respawnTime )
		{
			EnableAllCollisions = false;
			EnableDrawing = false;
			RespawnTime = respawnTime;
			Controller = null;
			Camera = new SpectateCamera
			{
				DeathPosition = position
			};
		}

		public override void ClientSpawn()
		{
			if ( IsLocalPawn )
			{
				SpeedLines = Particles.Create( "particles/player/speed_lines.vpcf" );
				RadarHud = Local.Hud.AddChild<Radar>();
			}

			base.ClientSpawn();
		}

		public override void StartTouch( Entity other )
		{
			if ( other is JetpackElevator && Controller is MoveController controller )
			{
				controller.InEnergyElevator = true;
			}

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( other is JetpackElevator && Controller is MoveController controller )
			{
				controller.InEnergyElevator = false;
			}

			base.EndTouch( other );
		}

		public override void Respawn()
		{
			base.Respawn();

			RemoveRagdollEntity();
			ClearAssistTrackers();
			StopJetpackLoop();
			StopSkiLoop();

			Rounds.Current?.OnPlayerSpawn( this );
		}

		public override void OnKilled()
		{
			base.OnKilled();

			var attacker = LastAttacker;

			if ( attacker.IsValid() )
			{
				if ( attacker is Player killer )
				{
					killer.OnKillPlayer( this, LastDamageInfo );
				}

				Rounds.Current?.OnPlayerKilled( this, attacker, LastDamageInfo );
			}
			else
			{
				Rounds.Current?.OnPlayerKilled( this, null, LastDamageInfo );
			}

			BecomeRagdollOnServer( LastDamageInfo.Force, GetHitboxBone( LastDamageInfo.HitboxIndex ) );
			Inventory.DeleteContents();

			StopJetpackLoop();
			StopSkiLoop();

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
				return;

			if ( Input.Released( InputButton.View ) )
			{
				if ( Camera is FirstPersonCamera )
					Camera = new ThirdPersonCamera();
				else
					Camera = new FirstPersonCamera();
			}

			if ( IsServer && Input.Released( InputButton.Drop ) )
			{
				foreach ( var flag in All.OfType<FlagEntity>() )
				{
					if ( flag.Carrier == this )
					{
						flag.Drop( true );
						break;
					}
				}
			}

			TickPlayerUse();

			var controller = GetActiveController();
			controller?.Simulate( client, this, GetActiveAnimator() );
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

		private void ClearAssistTrackers()
		{
			AssistTrackers.Clear();
		}

		private AssistTracker GetAssistTracker( Player attacker )
		{
			foreach ( var v in AssistTrackers )
			{
				if ( v.Attacker == attacker )
				{
					return v;
				}
			}

			var tracker = new AssistTracker
			{
				LastDamageTime = 0f,
				Attacker = attacker
			};

			AssistTrackers.Add( tracker );

			return tracker;
		}

		private void AddAssistDamage( Player attacker, DamageInfo info )
		{
			var tracker = GetAssistTracker( attacker );

			if ( tracker.LastDamageTime > 10f )
			{
				tracker.TotalDamage = 0f;
			}

			tracker.LastDamageTime = 0f;
			tracker.TotalDamage += info.Damage;
		}

		private void AddCameraEffects( ref CameraSetup setup )
		{
			if ( Controller is not MoveController controller )
				return;

			var forwardSpeed = Velocity.Normal.Dot( setup.Rotation.Forward );
			var speed = Velocity.Length.LerpInverse( 0, controller.MaxSpeed );
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
			if ( info.HitboxIndex == 0 && !info.Flags.HasFlag( DamageFlags.Blast ) )
			{
				info.Damage *= 2.0f;
			}

			if ( info.Attacker is Player attacker && attacker != this )
			{
				if ( !Game.AllowFriendlyFire )
				{
					return;
				}

				AddAssistDamage( attacker, info );

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

			IsRegenerating = false;
			LastDamageInfo = info;
			NextRegenTime = RegenDelay;

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

		public virtual void OnCaptureFlag( FlagEntity flag )
		{
			Client.SetInt( "captures", Client.GetInt( "captures", 0 ) + 1 );
			GiveAward<CaptureFlagAward>();
		}

		public virtual void OnReturnFlag( FlagEntity flag )
		{
			GiveAward<ReturnFlagAward>();
		}

		public virtual void OnKillPlayer( Player victim, DamageInfo damageInfo )
		{
			if ( LifeState == LifeState.Alive )
				KillStreak++;
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( IsLocalPawn && Controller is MoveController controller )
			{
				var speed = Velocity.Length.Remap( 0f, controller.MaxSpeed, 0f, 1f );
				speed = Math.Min( Easing.EaseIn( speed ) * 60f, 60f );
				SpeedLines.SetPosition( 1, new Vector3( speed, 0f, 0f ) );
			}
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( LifeState != LifeState.Alive && RespawnTime )
			{
				Respawn();
			}

			for ( int i = AssistTrackers.Count - 1; i >= 0; i-- )
			{
				var tracker = AssistTrackers[i];

				if ( tracker.LastDamageTime > 10f )
				{
					AssistTrackers.RemoveAt( i );
				}
			}

			CheckLowEnergy();
			UpdateHealthRegen();
			UpdateJetpackLoop();
			UpdateSkiLoop();
		}

		protected virtual void UpdateHealthRegen()
		{
			if ( LifeState == LifeState.Dead || !NextRegenTime )
				return;

			if ( Health == MaxHealth )
				return;

			if ( !IsRegenerating )
			{
				IsRegenerating = true;
				PlaySound( "regen.start" );
			}

			Health = Math.Clamp( Health + HealthRegen * Time.Delta, 0f, MaxHealth );
		}

		protected virtual void UpdateJetpackLoop()
		{
			if ( Controller is MoveController controller )
			{
				if ( IsPlayingJetpackLoop )
				{
					if ( !controller.IsJetpacking )
					{
						PlaySound( "jetpack.blast" ).SetVolume( 0.1f );
						StopJetpackLoop();
					}
				}
				else if ( controller.IsJetpacking )
				{
					StartJetpackLoop();
				}
			}
			else if ( IsPlayingJetpackLoop )
			{
				StopJetpackLoop();
			}
		}

		protected virtual void CheckLowEnergy()
		{
			if ( Controller is MoveController controller )
			{
				if ( controller.Energy < 5f && PlayLowEnergySound )
				{
					PlaySound( "regen.energylow" );
					PlayLowEnergySound = false;
				}
				else if ( controller.Energy > controller.MaxEnergy * 0.5f && !PlayLowEnergySound )
				{
					PlayLowEnergySound = true;
				}
			}
		}

		protected virtual void StopJetpackLoop()
		{
			IsPlayingJetpackLoop = false;
			JetpackLoop.Stop();
		}

		protected virtual void StartJetpackLoop()
		{
			IsPlayingJetpackLoop = true;
			JetpackLoop.Stop();
			JetpackLoop = PlaySound( "jetpack.fly" );
			PlaySound( "jetpack.blast" );
		}

		protected virtual void UpdateSkiLoop()
		{
			if ( Controller is MoveController controller )
			{
				if ( IsPlayingSkiLoop )
				{
					if ( !controller.IsSkiing || Velocity.Length < controller.MaxSpeed * 0.1f )
					{
						StopSkiLoop();
						PlaySound( "ski.stop" );
					}
					else
					{
						SkiLoop.SetVolume( Velocity.Length.Remap( 0f, controller.MaxSpeed, 0f, 0.4f ) );
					}
				}
				else if ( controller.IsSkiing && Velocity.Length > controller.MaxSpeed * 0.1f )
				{
					StartSkiLoop();
				}
			}
			else if ( IsPlayingSkiLoop )
			{
				StopSkiLoop();
			}
		}

		protected virtual void StopSkiLoop()
		{
			IsPlayingSkiLoop = false;
			SkiLoop.Stop();
		}

		protected virtual void StartSkiLoop()
		{
			IsPlayingSkiLoop = true;
			SkiLoop.Stop();
			SkiLoop = PlaySound( "ski.loop" );
		}

		protected override void UseFail()
		{
			// Do nothing. By default this plays a sound that we don't want.
		}

		protected override void OnDestroy()
		{
			RemoveRagdollEntity();
			StopJetpackLoop();
			StopSkiLoop();

			if ( IsLocalPawn )
			{
				SpeedLines?.Destroy();
				RadarHud?.Delete();
			}

			base.OnDestroy();
		}
	}
}
