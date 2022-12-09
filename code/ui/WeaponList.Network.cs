using Sandbox;

namespace Facepunch.Hover.UI;

public partial class WeaponList
{
	[ClientRpc]
	public static void Expand( float duration )
	{
		if ( Instance != null )
		{
			Instance.RemainOpenUntil = duration;
		}
	}
}
