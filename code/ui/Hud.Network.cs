using Sandbox;

namespace Facepunch.Hover.UI;

public partial class Hud
{
	[ClientRpc]
	public static void AddKillFeed( HoverPlayer attacker, HoverPlayer victim, Entity weapon )
	{
		ToastList.Instance.AddKillFeed( attacker, victim, weapon );
	}

	[ClientRpc]
	public static void AddKillFeed( Entity attacker, HoverPlayer victim )
	{
		ToastList.Instance.AddKillFeed( attacker, victim );
	}

	[ClientRpc]
	public static void AddKillFeed( HoverPlayer victim )
	{
		ToastList.Instance.AddKillFeed( victim );
	}

	public static void ToastAll( string text, string icon = "" )
	{
		Toast( To.Everyone, text, icon );
	}

	public static void Toast( HoverPlayer player, string text, string icon = "" )
	{
		Toast( To.Single( player ), text, icon );
	}

	[ClientRpc]
	public static void Toast( string text, string icon = "" )
	{
		ToastList.Instance.AddItem( text, Texture.Load( FileSystem.Mounted, icon ) );
	}
}
