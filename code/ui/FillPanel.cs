
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public enum FillDirection
    {
		Left,
		Right,
		Up,
		Down
    }

	public class FillPanel : Panel
	{
		public FillDirection Direction { get; set; } = FillDirection.Right;
		public Length? Fill { get; set; }

		public override void PreDrawBackground(ref RenderState state)
		{
			if (Fill.HasValue)
			{
				var f = Fill.Value;
				var rect = Box.Rect;
				var fw = f.GetPixels(rect.width);
				var fh = f.GetPixels(rect.height);
				var scissor = rect;

				if (Direction == FillDirection.Right)
					scissor.width = fw;
				else if (Direction == FillDirection.Down)
					scissor.height = fh;
				else if (Direction == FillDirection.Left)
					scissor.left += (rect.width - fw);
				else if (Direction == FillDirection.Up)
					scissor.bottom += (rect.height - fw);

				Sandbox.Render.ScissorRect = scissor;
			}
		}

		public override void PostDrawBackground(ref RenderState state)
		{
			Sandbox.Render.ScissorRect = new Rect(0f, 0f, Screen.Width, Screen.Height);
		}
	}
}
