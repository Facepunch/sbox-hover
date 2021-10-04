using Sandbox;

namespace Facepunch.Hover
{
	public partial class FlagEntity : ModelEntity, IHudEntity
	{
		public delegate void FlagEvent( Player player, FlagEntity flag );
		public static event FlagEvent OnFlagPickedUp;
		public static event FlagEvent OnFlagDropped;

		[Net] public RealTimeUntil NextPickupTime { get; private set; }
		[Net] public FlagSpawnpoint Spawnpoint { get; private set; }
		[Net] public Player Carrier { get; private set; }
		[Net] public bool IsAtHome { get; private set; }
		[Net] public Team Team { get; private set; }

		public FlagIndicator Indicator { get; private set; }
		public EntityHudAnchor Hud { get; private set; }
		public Vector3 LocalCenter => CollisionBounds.Center;
		public Particles Effects { get; private set; }

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
			Hud = EntityHud.Instance.Create( this );
			
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

		public void Drop()
		{
			if ( !IsAtHome && Carrier.IsValid() )
			{
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
			var boneTransform = player.GetBoneTransform( boneIndex );

			SetParent( player, boneIndex );

			LocalRotation = Rotation.From( 0f, 90f, 120f );
			LocalPosition = boneTransform.Rotation.Backward * 15f;
			Carrier = player;
			IsAtHome = false;

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
						GiveToPlayer( player );
					}
				}
				else
				{
					if ( NextPickupTime && !Carrier.IsValid() )
					{
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

		private void DoBaseEffects()
		{
			Effects?.Destroy();
			Effects = Particles.Create( "particles/flag/flag_idle_base.vpcf", this );
		}

		private void DoIdleEffects()
		{
			Effects?.Destroy();
			Effects = Particles.Create( "particles/flag/flag_idle_ground.vpcf", this );
		}

		private void DoTrailEffects()
		{
			Effects?.Destroy();
			Effects = Particles.Create( "particles/flag/flag_idle_trail.vpcf", this );
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( IsAtHome ) return;

			if ( Carrier.IsValid() && Carrier.LifeState == LifeState.Dead )
			{
				Drop();
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
					Rotation = Rotation.FromYaw( Rotation.Yaw() + 45f * Time.Delta );
					return;
				}

				trace = Trace.Ray( position, position + Vector3.Down * height * 4f )
					.Ignore( this )
					.Run();

				if ( trace.StartedSolid )
					return;

				Rotation = Rotation.Slerp( Rotation, Rotation.Identity, 10f * Time.Delta );
				Position = Position.LerpTo( trace.EndPos + height, 20f * Time.Delta );
			}
		}

		public bool ShouldUpdateHud()
		{
			return true;
		}

		public void UpdateHudComponents()
		{
			var distance = Local.Pawn.Position.Distance( Position ) - 1500f;
			var mapped = distance.Remap( 0f, 1000f, 0f, 1f );

			if ( Hud.Style.Opacity != mapped )
			{
				Hud.Style.Opacity = mapped;
				Hud.Style.Dirty();
			}
		}
	}
}
