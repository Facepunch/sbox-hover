using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class KillingSpreeAward : Award
	{
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/killing_spree.png" );
		public override string Name => "Killing Spree";
		public override string Description => "Kill at least 4 players before dying";
		public override int Tokens => 200;
	}
}
