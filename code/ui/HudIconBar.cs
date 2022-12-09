
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover.UI
{
	[StyleSheet( "/ui/HudIconBar.scss" )]
	public class HudIconBar : Panel
	{
		public Panel InnerBar;
		public Panel OuterBar;
		public Panel Icon;
		public Label Text;

		public HudIconBar()
		{
			OuterBar = Add.Panel( "outerBar" );
			InnerBar = OuterBar.Add.Panel( "innerBar" );
			Icon = Add.Panel( "icon" );
			Text = Add.Label( "0", "text" );
		}
	}
}
