using Sandbox.UI;

namespace Facepunch.Hover.UI
{
	[StyleSheet( "/ui/SimpleIconBar.scss" )]
	public class SimpleIconBar : Panel
	{
		public Panel InnerBar;
		public Panel OuterBar;
		public Panel Icon;

		public SimpleIconBar()
		{
			Icon = Add.Panel( "icon" );
			OuterBar = Add.Panel( "outerBar" );
			InnerBar = OuterBar.Add.Panel( "innerBar" );
		}
	}
}
