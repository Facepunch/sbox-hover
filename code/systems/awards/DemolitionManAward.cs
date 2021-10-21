using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class DemolitionManAward : Award
	{
		public override Texture Icon => Texture.Load( "ui/icons/skull.png" );
		public override string Name => "Demolition Man";
		public override string Description => "Destroy the enemy team's generator";
		public override int Tokens => 250;
	}
}
