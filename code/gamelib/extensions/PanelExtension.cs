using Sandbox.UI;
using Sandbox;
using System.Collections.Generic;

namespace Gamelib.Extensions
{
	public static class PanelExtension
	{
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
