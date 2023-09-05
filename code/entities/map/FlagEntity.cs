﻿using System;
using Sandbox;

namespace Facepunch.Hover
{
	public partial class FlagEntity : ModelEntity, IHudEntity, IGameResettable
	{
		public delegate void FlagEvent( HoverPlayer player, FlagEntity flag );
		public static event FlagEvent OnFlagReturned;
		public static event FlagEvent OnFlagPickedUp;
		public static event FlagEvent OnFlagDropped;

		[Net] public RealTimeUntil NextPickupTime { get; private set; }
		[Net] public FlagSpawnpoint Spawnpoint { get; private set; }
		[Net] public HoverPlayer Carrier { get; private set; }
		[Net] public bool IsAtHome { get; private set; }
		[Net] public Team Team { get; private set; }

		public UI.FlagIndicator Indicator { get; private set; }
		public UI.EntityHudAnchor Hud { get; private set; }
		public bool IsOnGround { get; private set; }
		public Vector3 CustomVelocity { get; private set; }
		public Vector3 LocalCenter => CollisionBounds.Center;
		public Particles Effects { get; private set; }

		public void OnGameReset()
		{
			Respawn();
		}

		public override void Spawn()
		{
			SetModel( "models/flag/temp_flag.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			EnableAllCollisions = false;
			EnableTouch = true;
			Tags.Add( "trigger" );

			Transmit = TransmitType.Always;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Hud = UI.EntityHud.Create( this );
			Hud.UpOffset = 40f;

			Indicator = Hud.AddChild<UI.FlagIndicator>( "flag" );
			Indicator.SetTeam( Team );

			base.ClientSpawn();
		}

		public void SetSpawnpoint( FlagSpawnpoint spawnpoint )
		{
			Spawnpoint = spawnpoint;
			SpawnAt( spawnpoint );
		}

		public void SetTeam( Team team )
		{
			Team = team;

			if ( team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;
		}

		public void SpawnAt( FlagSpawnpoint spawnpoint )
		{
			DoBaseEffects();
			SetParent( spawnpoint );
			NextPickupTime = 2f;
			LocalPosition = new Vector3( 0f, 0f, 60f );
			LocalRotation = Rotation.Identity;
			PlaySound( "flag.land" );
			IsAtHome = true;
			Carrier = null;
		}

		public void Respawn()
		{
			if ( Spawnpoint.IsValid() )
			{
				SpawnAt( Spawnpoint );
			}
		}

		public void Drop( bool inheritVelocity = false )
		{
			if ( !IsAtHome && Carrier.IsValid() )
			{
				if ( inheritVelocity )
				{
					CustomVelocity = Carrier.Velocity * 0.75f;

					var velocityToAdd = Carrier.Velocity * 0.25f;
					var trace = Trace.Ray( Position, Position + velocityToAdd )
						.WithAnyTags( "solid", "playerclip" )
						.Ignore( this )
						.Run();

					Position = trace.EndPosition;
				}
				else
				{
					CustomVelocity = 0f;
				}

				IsOnGround = false;
				DoIdleEffects();
				OnFlagDropped?.Invoke( Carrier, this );
				SetParent( null );
				Carrier = null;
			}
		}

		public void GiveToPlayer( HoverPlayer player )
		{
			DoTrailEffects();

			var boneIndex = player.GetBoneIndex( "spine_2" );

			SetParent( player, boneIndex );

			IsOnGround = true;
			Carrier = player;
			IsAtHome = false;

			UpdateLocalPosition();

			OnFlagPickedUp?.Invoke( player, this );
		}

		public override void StartTouch( Entity other )
		{
			if ( !HoverGame.Round.CanCaptureFlags ) return;

			if ( Game.IsServer && other is HoverPlayer player )
			{
				if ( player.Team == Team )
				{
					if ( !IsAtHome && !Carrier.IsValid() )
					{
						PlaySound( "flag.capture" );
						OnFlagReturned?.Invoke( player, this );
						player.OnReturnFlag( this );
						Respawn();
					}
				}
				else
				{
					if ( NextPickupTime && !Carrier.IsValid() )
					{
						if ( player.NextCanPickupFlag )
						{
							PlaySound( "flag.pickup" );
							GiveToPlayer( player );
						}
					}
				}
			}

			base.StartTouch( other );
		}

		protected override void OnDestroy()
		{
			Hud?.Delete( true );
			Hud = null;

			base.OnDestroy();
		}

		private void UpdateLocalPosition()
		{
			LocalRotation = Rotation.From( 0f, 90f, 120f );
			LocalPosition = new Vector3( 0f, -15, 0f );
		}

		private void DoBaseEffects()
		{
			Effects?.Destroy();
			Effects = Particles.Create( "particles/flag/flag_idle_base.vpcf", this );
			Effects.SetPosition( 1, Team.GetColor() * 255f );
		}

		private void DoIdleEffects()
		{
			Effects?.Destroy();
			Effects = Particles.Create( "particles/flag/flag_idle_ground.vpcf", this );
			Effects.SetPosition( 1, Team.GetColor() * 255f );
		}

		private void DoTrailEffects()
		{
			Effects?.Destroy();
			Effects = Particles.Create( "particles/flag/flag_idle_trail.vpcf", this );
			Effects.SetPosition( 1, Team.GetColor() * 255f );
		}

		[GameEvent.Tick.Server]
		private void ServerTick()
		{
			if ( IsAtHome ) return;

			if ( Carrier.IsValid() && Carrier.LifeState == LifeState.Dead )
			{
				Drop( true );
				return;
			}

			if ( !Carrier.IsValid() )
			{
				const float height = 60f;
				
				var position = Position.WithZ( Position.z + height );

				var trace = Trace.Ray( position, position + Vector3.Down * height * 2f )
					.WithAnyTags( "solid", "playerclip" )
					.Ignore( this )
					.Run();

				if ( trace.Hit || trace.StartedSolid )
				{
					if ( !IsOnGround )
					{
						IsOnGround = true;
						PlaySound( "flag.land" );
					}

					Rotation = Rotation.FromYaw( Rotation.Yaw() + 90f * Time.Delta );
					return;
				}

				trace = Trace.Ray( position, position + CustomVelocity * Time.Delta )
					.WithAnyTags( "solid", "playerclip" )
					.Ignore( this )
					.Run();

				if ( trace.Hit || trace.StartedSolid )
				{
					CustomVelocity = Vector3.Reflect( CustomVelocity.Normal, trace.Normal ) * CustomVelocity.Length * 0.5f;
					return;
				}

				CustomVelocity -= (CustomVelocity * 0.05f * Time.Delta);

				Rotation = Rotation.Slerp( Rotation, Rotation.Identity, Time.Delta );
				Position += CustomVelocity * Time.Delta;
				Position += Vector3.Down * 600f * Time.Delta;
			}
			else
			{
				UpdateLocalPosition();
			}
		}

		[GameEvent.Tick.Client]
		private void ClientTick()
		{
			// Hide the flag if we're the current carrier. It can be annoying.
			if ( Carrier.IsValid() && Carrier.IsLocalPawn )
				EnableHideInFirstPerson = true;
			else
				EnableHideInFirstPerson = false;

			EnableShadowInFirstPerson = true;

			if ( !Hud.IsValid() ) return;
			Hud.UpOffset = Carrier.IsValid() ? 96f : 40f;
		}

		public bool ShouldUpdateHud()
		{
			return true;
		}

		public void UpdateHudComponents()
		{
			var distance = Game.LocalPawn.Position.Distance( Position ) - 1500f;
			var mapped = distance.Remap( 0f, 1000f, 0f, 1f ).Clamp( 0f, 1f );

			if ( Hud.Style.Opacity.HasValue && Math.Abs( Hud.Style.Opacity.Value - mapped ) <= 0.001f )
				return;

			Hud.Style.Opacity = mapped;
			Hud.Style.Dirty();
		}
	}
}
