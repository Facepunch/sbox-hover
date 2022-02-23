using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class RevengeAward : Award
	{
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/revenge.png" );
		public override string Name => "Revenge";
		public override string Description => "Kill the player who last killed you";
		public override int Tokens => 150;
	}
}
