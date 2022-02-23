
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class FlagIndicator : EntityHudIcon
	{
		public FlagIndicator() : base()
		{
			StyleSheet.Load( "/ui/FlagIndicator.scss" );
		}

		public void SetTeam( Team team )
		{
			if ( team == Team.Blue )
				Texture = Texture.Load( FileSystem.Mounted, "ui/icons/flag-blue.png" );
			else
				Texture = Texture.Load( FileSystem.Mounted, "ui/icons/flag-red.png" );
		}
	}
}
