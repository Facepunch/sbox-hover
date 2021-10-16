
using Gamelib.Maths;
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
			var halfScopeSize = scopeSize * 0.5f;
			var halfScreenX = screenSize.x * 0.5f;
			var halfScreenY = screenSize.y * 0.5f;

			Inner.SetTexture( "ui/scope_inner.png" );

			Inner.Style.Width = scopeSize;
			Inner.Style.Height = scopeSize;
			Inner.Style.Left = halfScreenX - halfScopeSize;
			Inner.Style.Top = halfScreenY - halfScopeSize;

			Left.Style.Width = halfScreenX - halfScopeSize;
			Left.Style.Top = halfScopeSize;
			Left.Style.Left = 0f;
			Left.Style.Height = screenSize.y - scopeSize;

			Right.Style.Width = Length.Percent( 100f );
			Right.Style.Left = halfScreenX + halfScopeSize;
			Right.Style.Top = halfScopeSize;
			Right.Style.Height = screenSize.y - scopeSize;

			Top.Style.Top = 0f;
			Top.Style.Width = screenSize.x;
			Top.Style.Height = halfScopeSize;
			Top.Style.Left = 0f;

			Bottom.Style.Top = halfScreenY + halfScopeSize;
			Bottom.Style.Width = screenSize.x;
			Bottom.Style.Height = Length.Percent( 100f );
			Bottom.Style.Left = 0f;

			base.Tick();
		}
	}
}
