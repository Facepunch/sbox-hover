using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class AssistAward : Award
	{
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/assist.png" );
		public override string Name => "Assist";
		public override string Description => "Assist your team in killing an enemy player";
		public override int Tokens => 100;
	}
}
