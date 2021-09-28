using Sandbox;

namespace Facepunch.Hover
{
	public partial class FlagEntity : ModelEntity
	{
		[Net] public FlagSpawnpoint Spawnpoint { get; private set; }
		[Net] public Player Carrier { get; private set; }
		[Net] public bool IsAtHome { get; private set; }
		[Net] public TeamType Team { get; private set; }

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

		public void SetTeam( TeamType team )
		{
			Team = team;

			if ( team == TeamType.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;
		}

		public void SpawnAt( FlagSpawnpoint spawnpoint )
		{
			// TODO: Do this with attachments and shit.
			Position = spawnpoint.Position + new Vector3( 0f, 0f, 60f );
			SetParent( spawnpoint );
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
			Carrier = player;
			IsAtHome = false;
			SetParent( player, player.GetBoneIndex( "spine_2" ) );
			LocalRotation = Rotation.From( 0f, 90f, 120f );
			LocalPosition = player.Rotation.Left * 10f;
		}

		public override void StartTouch( Entity other )
		{
			if ( other is Player player )
			{
				if ( player.Team.Type == Team )
				{
					if ( !IsAtHome && !Carrier.IsValid() )
					{
						GiveToPlayer( player );
					}
				}
				else
				{
					if ( !Carrier.IsValid() )
					{
						GiveToPlayer( player );
					}
				}
			}

			base.StartTouch( other );
		}
	}
}
