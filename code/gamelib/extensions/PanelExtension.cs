using Sandbox.UI;
using Sandbox;
using System.Collections.Generic;

namespace Gamelib.Extensions
{
	public static class PanelExtension
	{
		public static void PositionAtCrosshair( this Panel panel )
		{
			panel.PositionAtCrosshair( Local.Pawn as Player );
		}

		public static void PositionAtCrosshair( this Panel panel, Player player )
		{
			if ( !player.IsValid() ) return;

			var eyePos = player.EyePos;
			var eyeRot = player.EyeRot;

			var tr = Trace.Ray( eyePos, eyePos + eyeRot.Forward * 1000 )
				.Size( 1.0f )
				.Ignore( player )
				.UseHitboxes()
				.Run();

			panel.PositionAtWorld( tr.EndPos );
		}

		public static void PositionAtWorld( this Panel panel, Vector3 position )
		{
			var screenPos = position.ToScreen();

			if ( screenPos.z < 0 )
				return;

			panel.Style.Left = Length.Fraction( screenPos.x );
			panel.Style.Top = Length.Fraction( screenPos.y );
			panel.Style.Dirty();
		}

		public static IEnumerable<T> GetAllChildrenOfType<T>( this Panel panel ) where T : Panel
		{
			foreach ( var child in panel.Children )
			{
				if ( child is T )
				{
					yield return child as T;
				}

				foreach ( var v in child.GetAllChildrenOfType<T>() )
				{
					yield return v;
				}
			}
		}
	}
}
