using Sandbox;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.Hover
{
	[Library( "hv_spawnpoint" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
	[Display( Name = "Player Spawnpoint", GroupName = "Hover" )]
	public partial class PlayerSpawnpoint : Entity
	{
		[Property] public Team Team { get; set; }
	}
}
