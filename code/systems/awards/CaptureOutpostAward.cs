using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class CaptureOutpostAward : Award
	{
		public override Texture Icon => Texture.Load( "ui/icons/blue_outpost.png" );
		public override string Name => "Outpost Captured";
		public override string Description => "Help your team to capture an Outpost";
		public override int Tokens => 150;

		public override Texture GetShowIcon()
		{
			if ( Local.Pawn is Player player )
			{
				if ( player.Team == Team.Red )
					return Texture.Load( "ui/icons/red_outpost.png" );
			}

			return base.GetShowIcon();
		}
	}
}
