using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class KillAward : Award
	{
		public override Texture Icon => Texture.Load( "ui/icons/skull.png" );
		public override string Name => "Kill";
		public override string Description => "Kill an enemy player";
		public override int Tokens => 100;
	}
}
