using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_spawnpoint" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Player Spawnpoint", "Hover", "Defines a point where players on a team can spawn" )]
	public partial class PlayerSpawnpoint : Entity
	{
		[Property] public Team Team { get; set; }
	}
}
