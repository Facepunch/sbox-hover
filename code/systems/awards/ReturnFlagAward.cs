using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class ReturnFlagAward : Award
	{
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/blue_return_flag.png" );
		public override string Name => "Returned the Flag";
		public override string Description => "Safely return your team's flag to your home base";
		public override bool TeamReward => true;
		public override int Tokens => 200;

		public override Texture GetShowIcon()
		{
			if ( Local.Pawn is HoverPlayer player )
			{
				if ( player.Team == Team.Red )
					return Texture.Load( FileSystem.Mounted, "ui/awards/red_return_flag.png" );
			}

			return base.GetShowIcon();
		}
	}
}
