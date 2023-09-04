using Sandbox;

namespace Facepunch.Hover.UI;

public partial class RespawnScreen
{
	[ClientRpc]
	public static void Show( float respawnTime, Entity attacker, Entity weapon = null )
	{
		if ( !Instance.IsValid() ) return;
		
		Instance.SetClass( "hidden", false );

		if ( attacker is HoverPlayer player )
			Instance.KillerInfo.Update( player );
		else if ( attacker.IsWorld )
			Instance.KillerInfo.Update( "Unknown" );
		else if ( attacker is IKillFeedIcon killer )
			Instance.KillerInfo.Update( killer );
		else
			Instance.KillerInfo.Update( attacker.Name );

		Instance.KillerInfo.SetWeapon( weapon );
		Instance.RespawnTime = respawnTime;
	}

	[ClientRpc]
	public static void Hide()
	{
		Instance?.SetClass( "hidden", true );
	}
}
