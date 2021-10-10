using Sandbox.UI;

namespace Facepunch.Hover
{
	public class SimpleIconBar : Panel
	{
		public Panel InnerBar;
		public Panel OuterBar;
		public Panel Icon;

		public SimpleIconBar()
		{
			StyleSheet.Load( "/ui/SimpleIconBar.scss" );

			Icon = Add.Panel( "icon" );
			OuterBar = Add.Panel( "outerBar" );
			InnerBar = OuterBar.Add.Panel( "innerBar" );
		}
	}
}
