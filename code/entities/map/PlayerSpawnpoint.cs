using Sandbox;
using Editor;

namespace Facepunch.Hover
{
	[Library( "hv_spawnpoint" )]
	[EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
	[Title( "Player Spawnpoint" )]
	[HammerEntity]
	public partial class PlayerSpawnpoint : Entity
	{
		[Property] public Team Team { get; set; }
	}
}
