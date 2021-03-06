using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class BuzzkillAward : Award
	{
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/buzzkill.png" );
		public override string Name => "Buzzkill";
		public override string Description => "Kill an enemy who has a high kill streak";
		public override int Tokens => 300;
	}
}
