
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class LongshotScope : Panel
	{
		public static LongshotScope Instance { get; private set; }

		public Panel Left { get; private set; }
		public Panel Right { get; private set; }
		public Panel Top { get; private set; }
		public Panel Bottom { get; private set; }
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

			Left = Add.Panel( "left" );
			Right = Add.Panel( "right" );
			Top = Add.Panel( "top" );
			Bottom = Add.Panel( "bottom" );
			Inner = Add.Image( "ui/scope_inner.png", "inner" );

			Instance = this;

			Hide();
		}

		public override void Tick()
		{
			var screenSize = Box.Rect.Size * ScaleFromScreen;
			var scopeSize = screenSize.y * 0.5f;

			Inner.Style.Width = scopeSize;
			Inner.Style.Height = scopeSize;
			Inner.Style.Left = Length.Pixels( screenSize.x / 2f );
			Inner.Style.Top = Length.Pixels( screenSize.y / 2f );

			Left.Style.Width = ((screenSize.x * 0.5f) - (scopeSize * 0.5f)) + 1f;
			Left.Style.Top = scopeSize * 0.5f;
			Left.Style.Left = 0f;
			Left.Style.Height = screenSize.y - (scopeSize * 0.5f);

			Right.Style.Width = Left.Style.Width;
			Right.Style.Left = ((screenSize.x * 0.5f) + (scopeSize * 0.5f)) - 1f;
			Right.Style.Top = scopeSize * 0.5f;
			Right.Style.Height = screenSize.y - (scopeSize * 0.5f);

			Top.Style.Top = 0f;
			Top.Style.Width = screenSize.x;
			Top.Style.Height = (scopeSize * 0.5f) + 1f;
			Top.Style.Left = 0f;

			Bottom.Style.Bottom = 0f;
			Bottom.Style.Width = screenSize.x;
			Bottom.Style.Height = (scopeSize * 0.5f) + 1f;
			Bottom.Style.Left = 0f;

			base.Tick();
		}
	}
}
