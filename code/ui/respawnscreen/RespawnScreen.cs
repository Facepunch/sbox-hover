using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	[UseTemplate] 
	public partial class RespawnScreen : Panel
	{
		public static RespawnScreen Instance { get; private set; }

		public RespawnKillerInfo KillerInfo { get; private set; }
		public Label RespawnTimeLabel { get; private set; }
		public Label KilledByLabel { get; private set; }

		public RealTimeUntil RespawnTime { get; private set; }

		public string RespawnTimeLeft => $"{Math.Max( RespawnTime.Relative.CeilToInt(), 0 )}s";

		[ClientRpc]
		public static void Show( float respawnTime, Entity attacker, Entity weapon = null )
		{
			Instance.SetClass( "hidden", false );

			if ( attacker is Player player )
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
			Instance.SetClass( "hidden", true );
		}

		public RespawnScreen()
		{
			SetClass( "hidden", true );

			Instance = this;
		}
	}
}
