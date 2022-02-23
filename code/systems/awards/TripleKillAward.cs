using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class TripleKillAward : Award
	{
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/triple_kill.png" );
		public override string Name => "Triple Kill";
		public override string Description => "Kill three players within a short time";
		public override int Tokens => 200;
	}
}
