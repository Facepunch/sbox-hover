using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_flag_spawnpoint" )]
	[Hammer.EditorModel( "models/flag/temp_flag_base.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Flag Spawnpoint", "Hover", "Defines a point where a team's flag spawns" )]
	public partial class FlagSpawnpoint : ModelEntity
	{
		[Property] public TeamType Team { get; set; }

		[Net] public FlagEntity Flag { get; set; }

		public override void Spawn()
		{
			SetModel( "models/flag/temp_flag_base.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			EnableAllCollisions = false;
			EnableTouch = true;

			Transmit = TransmitType.Always;

			if ( Team == TeamType.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

			Flag = new FlagEntity();
			Flag.SetTeam( Team );
			Flag.SetSpawnpoint( this );
			Flag.Respawn();

			base.Spawn();
		}

		public override void StartTouch( Entity other )
		{
			if ( other is FlagEntity flag && flag.Carrier.IsValid() )
			{
				if ( flag.Team == Team )
				{
					// TODO: Well done we got it home, boys!
				}
				else
				{
					// TODO: Well done we scored a point!
				}

				flag.Respawn();
			}

			base.StartTouch( other );
		}
	}
}
