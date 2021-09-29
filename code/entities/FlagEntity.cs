using Sandbox;

namespace Facepunch.Hover
{
	public partial class FlagEntity : ModelEntity
	{
		[Net] public RealTimeUntil NextPickupTime { get; private set; }
		[Net] public FlagSpawnpoint Spawnpoint { get; private set; }
		[Net] public Player Carrier { get; private set; }
		[Net] public bool IsAtHome { get; private set; }
		[Net] public Team Team { get; private set; }

		public override void Spawn()
		{
			SetModel( "models/flag/temp_flag.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			EnableAllCollisions = false;
			EnableTouch = true;

			Transmit = TransmitType.Always;

			base.Spawn();
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

		public void GiveToPlayer( Player player )
		{
			var boneIndex = player.GetBoneIndex( "spine_2" );
			var boneTransform = player.GetBoneTransform( boneIndex );

			SetParent( player, boneIndex );

			LocalRotation = Rotation.From( 0f, 90f, 120f );
			LocalPosition = boneTransform.Rotation.Backward * 15f;
			Carrier = player;
			IsAtHome = false;
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
	}
}
