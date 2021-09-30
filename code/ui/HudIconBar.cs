
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class HudIconBar : Panel
	{
		public Panel InnerBar;
		public Panel OuterBar;
		public Panel Icon;
		public Label Text;

		public HudIconBar()
		{
			StyleSheet.Load( "/ui/HudIconBar.scss" );

			OuterBar = Add.Panel( "outerBar" );
			InnerBar = OuterBar.Add.Panel( "innerBar" );
			Icon = Add.Panel( "icon" );
			Text = Add.Label( "0", "text" );
		}
	}
}
