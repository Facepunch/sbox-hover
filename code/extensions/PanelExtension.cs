using Sandbox.UI;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover;

public static class PanelExtension
{
	public static void PositionAtCrosshair( this Panel panel )
	{
		panel.PositionAtCrosshair( Game.LocalPawn as HoverPlayer );
	}

	public static void PositionAtCrosshair( this Panel panel, HoverPlayer player )
	{
		if ( !player.IsValid() ) return;
		
		var trace = Trace.Ray( player.EyePosition, player.EyePosition + player.EyeRotation.Forward * 1000f )
			.Size( 1f )
			.Ignore( player )
			.Ignore( player.ActiveChild )
			.Run();

		panel.PositionAtWorld( trace.EndPosition );
	}

	public static void PositionAtWorld( this Panel panel, Vector3 position )
	{
		var screenPos = position.ToScreen();

		if ( screenPos.z < 0f )
			return;

		panel.Style.Left = Length.Fraction( screenPos.x );
		panel.Style.Top = Length.Fraction( screenPos.y );
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
