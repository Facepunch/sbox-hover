using Sandbox;
using System;
using SandboxEditor;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_flag_spawnpoint" )]
	[EditorModel( "models/flag/temp_flag_base.vmdl", FixedBounds = true )]
	[Title( "Flag Spawnpoint" )]
	[HammerEntity]
	public partial class FlagSpawnpoint : ModelEntity
	{
		public delegate void FlagEvent( Player player, FlagEntity flag );
		public static event FlagEvent OnFlagCaptured;

		[Property] public Team Team { get; set; }

		[Net] public FlagEntity Flag { get; set; }

		public static FlagSpawnpoint GetForTeam( Team team )
		{
			return All.OfType<FlagSpawnpoint>().Where( v => v.Team == team ).FirstOrDefault();
		}

		public override void Spawn()
		{
			SetModel( "models/flag/temp_flag_base.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

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

		public bool CanCaptureFlag( Player player, FlagEntity flag )
		{
			if ( flag.Team == Team)
				return false;

			var homeSpawnpoint = GetForTeam( player.Team );

			if ( homeSpawnpoint == null )
				return false;

			return homeSpawnpoint.Flag.IsAtHome;
		}

		public override void StartTouch( Entity other )
		{
			if ( IsServer && other is FlagEntity flag && flag.Carrier.IsValid() )
			{
				if ( CanCaptureFlag( flag.Carrier, flag ) )
				{
					PlaySound( "flag.capture" );
					OnFlagCaptured?.Invoke( flag.Carrier, flag );
					flag.Carrier.OnCaptureFlag( flag );
					flag.Respawn();
				}
			}

			base.StartTouch( other );
		}
	}
}
