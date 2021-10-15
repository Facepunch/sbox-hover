
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class LongshotScope : Panel
	{
		public static LongshotScope Instance { get; private set; }

		public Image Inner { get; private set; }

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
			StyleSheet.Load( "/ui/LongshotScope.scss" );

			Inner = Add.Image( "ui/scope_inner.png", "inner" );

			Instance = this;

			Hide();
		}
	}
}
