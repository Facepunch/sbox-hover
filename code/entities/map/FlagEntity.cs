using Sandbox;

namespace Facepunch.Hover
{
	public partial class FlagEntity : ModelEntity, IHudEntity, IGameResettable
	{
		public delegate void FlagEvent( Player player, FlagEntity flag );
		public static event FlagEvent OnFlagReturned;
		public static event FlagEvent OnFlagPickedUp;
		public static event FlagEvent OnFlagDropped;

		[Net] public RealTimeUntil NextPickupTime { get; private set; }
		[Net] public FlagSpawnpoint Spawnpoint { get; private set; }
		[Net] public Player Carrier { get; private set; }
		[Net] public bool IsAtHome { get; private set; }
		[Net] public Team Team { get; private set; }

		public FlagIndicator Indicator { get; private set; }
		public EntityHudAnchor Hud { get; private set; }
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

			Transmit = TransmitType.Always;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Hud = EntityHud.Create( this );
			Hud.UpOffset = 40f;

			Indicator = Hud.AddChild<FlagIndicator>( "flag" );
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
					Position += Carrier.Velocity * 0.25f;
				}
				else
				{
					CustomVelocity = 0f;
				}

				NextPickupTime = 0.5f;
				IsOnGround = false;
				DoIdleEffects();
				OnFlagDropped?.Invoke( Carrier, this );
				SetParent( null );
				Carrier = null;
			}
		}

		public void GiveToPlayer( Player player )
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
			if ( IsServer && other is Player player )
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
						PlaySound( "flag.pickup" );
						GiveToPlayer( player );
					}
				}
			}

			base.StartTouch( other );
		}

		protected override void OnDestroy()
		{
			Hud?.Delete();

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

		[Event.Tick.Server]
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
				var height = 60f;
				var position = Position.WithZ( Position.z + height );

				var trace = Trace.Ray( position, position + Vector3.Down * height * 2f )
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
					.Ignore( this )
					.Run();

				if ( trace.Hit || trace.StartedSolid )
				{
					Position = trace.EndPos + height;
					return;
				}

				CustomVelocity -= (CustomVelocity * 0.2f * Time.Delta);

				Rotation = Rotation.Slerp( Rotation, Rotation.Identity, Time.Delta );
				Position += CustomVelocity * Time.Delta;
				Position += Vector3.Down * 600f * Time.Delta;
			}
			else
			{
				UpdateLocalPosition();
			}
		}

		public bool ShouldUpdateHud()
		{
			return true;
		}

		public void UpdateHudComponents()
		{
			var distance = Local.Pawn.Position.Distance( Position ) - 1500f;
			var mapped = distance.Remap( 0f, 1000f, 0f, 1f ).Clamp( 0f, 1f );

			if ( Hud.Style.Opacity != mapped )
			{
				Hud.Style.Opacity = mapped;
				Hud.Style.Dirty();
			}
		}
	}
}
