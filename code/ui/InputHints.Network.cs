using Sandbox;

namespace Facepunch.Hover.UI;

public partial class InputHints
{
	[ClientRpc]
	public static void UpdateOnClient()
	{
		Current?.Update();
	}
}
