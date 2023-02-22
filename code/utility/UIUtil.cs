using Sandbox;
using Sandbox.UI;

namespace Facepunch.Hover;

public static class UIUtil
{
	public static float GetMinMaxDistanceAlpha( float distance, float fadeOutStart, float fadeOutEnd )
	{
		var mapped = 1f - distance.Remap( fadeOutStart, fadeOutEnd, 0f, 1f );
		return mapped.Clamp( 0f, 1f );
	}

	public static float GetMinMaxDistanceAlpha( float distance, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd )
        {
		distance -= fadeInStart;

		var mapped = distance.Remap( fadeInEnd, fadeInStart, 0f, 1f );

		if ( distance >= fadeOutStart)
		{
			mapped = 1f - distance.Remap( fadeOutStart, fadeOutEnd, 0f, 1f );
		}

		return mapped.Clamp( 0f, 1f );
	}

	public static Panel GetHoveredPanel( Panel root = null )
	{
		root ??= Game.RootPanel;

		if ( root.PseudoClass.HasFlag( PseudoClass.Hover ) )
		{
			if ( root.ComputedStyle.PointerEvents.HasValue )
			{
				if ( root.ComputedStyle.PointerEvents == PointerEvents.All )
					return root;
			}
		}

		foreach ( var child in root.Children )
		{
			var panel = GetHoveredPanel( child );

			if ( panel != null )
				return panel;
		}

		return null;
	}
}
