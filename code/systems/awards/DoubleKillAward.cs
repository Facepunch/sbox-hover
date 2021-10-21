using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class DoubleKillAward : Award
	{
		public override Texture Icon => Texture.Load( "ui/awards/double_kill.png" );
		public override string Name => "Double Kill";
		public override string Description => "Kill two players within a short time";
		public override int Tokens => 200;
	}
}
