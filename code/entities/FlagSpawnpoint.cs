using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library( "hv_flag_spawnpoint" )]
	[Hammer.EditorModel( "models/flag/temp_flag_base.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Flag Spawnpoint", "Hover", "Defines a point where a team's flag spawns" )]
	public partial class FlagSpawnpoint : ModelEntity
	{
		public delegate void FlagEvent( Player player, FlagEntity flag );
		public static event FlagEvent OnFlagReturned;
		public static event FlagEvent OnFlagCaptured;

		[Property] public Team Team { get; set; }

		[Net] public FlagEntity Flag { get; set; }

		public override void Spawn()
		{
			SetModel( "models/flag/temp_flag_base.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			EnableAllCollisions = false;
			EnableTouch = true;

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
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
			if ( IsServer && other is FlagEntity flag && flag.Carrier.IsValid() )
			{
				if ( flag.Team == Team && flag.Carrier.Team == Team )
				{
					OnFlagReturned?.Invoke( flag.Carrier, flag );
					flag.Carrier.OnReturnFlag( flag );
					flag.Respawn();
				}
				else if ( flag.Team != Team && flag.Carrier.Team == Team )
				{
					OnFlagCaptured?.Invoke( flag.Carrier, flag );
					flag.Carrier.OnCaptureFlag( flag );
					flag.Respawn();
				}
			}

			base.StartTouch( other );
		}
	}
}
