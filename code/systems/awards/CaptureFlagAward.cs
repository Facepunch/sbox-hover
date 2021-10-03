using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class CaptureFlagAward : Award
	{
		public override Texture Icon => Texture.Load( "ui/icons/flag-blue.png" );
		public override string Name => "Captured the Flag";
		public override string Description => "Safely bring the enemy flag to your home base";
		public override int Tokens => 500;

		public override Texture GetShowIcon()
		{
			if ( Local.Pawn is Player player )
			{
				if ( player.Team == Team.Blue )
					return Texture.Load( "ui/icons/flag-red.png" );
			}

			return base.GetShowIcon();
		}
	}
}
