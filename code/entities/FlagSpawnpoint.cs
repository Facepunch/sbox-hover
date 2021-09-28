using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_flag_spawnpoint" )]
	[Hammer.EditorModel( "models/flag/temp_flag_base.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Flag Spawnpoint", "Hover", "Defines a point where a team's flag spawns" )]
	public partial class FlagSpawnpoint : ModelEntity
	{
		[Property] public TeamType Team { get; set; }

		public override void Spawn()
		{
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			if ( Team == TeamType.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

			base.Spawn();
		}
	}
}
