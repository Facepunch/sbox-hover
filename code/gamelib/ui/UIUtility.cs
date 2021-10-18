using Sandbox;
using Sandbox.UI;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace Gamelib.UI
{
	public static class UIUtility
	{
		public static float GetMinMaxDistanceAlpha( float distance, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd )
        {
			distance -= fadeInStart;

			var mapped = distance.Remap( fadeInEnd, fadeInStart, 0f, 1f );

			if ( distance >= fadeOutStart)
			{
				mapped = 1f - distance.Remap( fadeOutStart, fadeOutEnd, 0f, 1f );
			}

			return mapped;
		}

		public static Panel GetHoveredPanel( Panel root = null )
		{
			root ??= Local.Hud;

			if ( root.PseudoClass.HasFlag( PseudoClass.Hover ) )
			{
				if ( !string.IsNullOrEmpty( root.ComputedStyle.PointerEvents ) )
				{
					if ( root.ComputedStyle.PointerEvents != "visible" && root.ComputedStyle.PointerEvents != "none" )
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
}
