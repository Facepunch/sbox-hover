using Sandbox;

namespace Facepunch.Hover.UI;

public partial class TutorialScreen
{
	[ConVar.Client( "hv_always_show_tutorial", Saved = true )]
	public static bool AlwaysShowTutorial { get; set; }

	[ConCmd.Client( "hv_clear_cookies" )]
	public static void ClearCookies()
	{
		Cookie.Set( "tutorial", false );
	}

	[ClientRpc]
	public static void Show()
	{
		if ( !AlwaysShowTutorial && Cookie.Get( "tutorial", false ) )
		{
			// Don't show them the screen again.
			return;
		}

		if ( Instance == null )
		{
			ShowWhenCreated = true;
			return;
		}

		Instance.SetClass( "hidden", false );
		Instance.HideTime = 5f;
	}

	[ClientRpc]
	public static void Hide()
	{
		if ( Instance == null ) return;

		Instance.SetClass( "hidden", true );
		Cookie.Set( "tutorial", true );
	}
}
