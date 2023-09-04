﻿using Sandbox;

namespace Facepunch.Hover.UI;

public partial class StationScreen
{
	[ClientRpc]
	public static void Refresh()
	{
		if ( ( Instance?.IsOpen ?? false ) && Game.LocalPawn is HoverPlayer player )
		{
			Instance.LoadoutList.Populate( player );
		}
	}

	[ClientRpc]
	public static void Toggle()
	{
		Instance?.SetOpen( !Instance.IsOpen );
	}

	[ClientRpc]
	public static void Show( StationScreenMode mode )
	{
		Instance?.SetMode( mode );
		Instance?.SetOpen( true );
	}

	[ClientRpc]
	public static void Hide()
	{
		Instance?.SetOpen( false );
	}
}
