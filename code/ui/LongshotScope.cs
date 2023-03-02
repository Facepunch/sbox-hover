using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover.UI
{
	[StyleSheet( "/ui/LongshotScope.scss" )]
	public class LongshotScope : Panel
	{
		public static LongshotScope Instance { get; private set; }

		public Panel Inner { get; private set; }

		public void Show()
		{
			SetClass( "hidden", false );
		}

		public void Hide()
		{
			SetClass( "hidden", true );
		}

		public LongshotScope()
		{
			Inner = Add.Panel( "inner" );

			Instance = this;

			Hide();
		}
	}
}
