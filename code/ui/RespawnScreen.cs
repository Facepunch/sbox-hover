using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public class RespawnKillerInfo : Panel
	{
		public Image KillerAvatar { get; private set; }
		public Label KillerName { get; private set; }

		public RespawnKillerInfo()
		{
			KillerAvatar = Add.Image( "", "avatar" );
			KillerName = Add.Label( "", "name" );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}

		public void Update( Player player )
		{
			KillerAvatar.SetTexture( $"avatar:{player.Client.SteamId}" );
			KillerName.Text = player.Client.Name;
			KillerName.Style.FontColor = player.Team.GetColor();
			KillerName.Style.Dirty();
		}

		public void Update( string killerName )
		{
			KillerAvatar.Texture = Texture.Load( "ui/icons/skull.png" );
			KillerName.Text = killerName;
			KillerName.Style.FontColor = Color.White;
			KillerName.Style.Dirty();
		}
	}

	public partial class RespawnScreen : Panel
	{
		public static RespawnScreen Instance { get; private set; }

		public RespawnKillerInfo KillerInfo { get; private set; }
		public Label RespawnTimeLabel { get; private set; }
		public Label KilledByLabel { get; private set; }

		public RealTimeUntil RespawnTime { get; private set; }

		[ClientRpc]
		public static void Show( RealTimeUntil respawnTime, Entity attacker )
		{
			Instance.SetClass( "hidden", false );

			if ( attacker is Player player )
				Instance.KillerInfo.Update( player );
			else if ( attacker.IsWorld )
				Instance.KillerInfo.Update( "Unknown" );
			else
				Instance.KillerInfo.Update( attacker.Name );

			Instance.RespawnTime = respawnTime;
		}

		[ClientRpc]
		public static void Hide()
		{
			Instance.SetClass( "hidden", true );
		}

		public RespawnScreen()
		{
			StyleSheet.Load( "/ui/RespawnScreen.scss" );

			RespawnTimeLabel = Add.Label( "", "respawn" );
			KilledByLabel = Add.Label( "You were killed by", "killedby" );
			KillerInfo = AddChild<RespawnKillerInfo>( "killer" );

			SetClass( "hidden", true );

			Instance = this;
		}

		public override void Tick()
		{
			var respawnTimeLeft = RespawnTime.Relative.CeilToInt();

			if ( respawnTimeLeft > 0 )
			{
				RespawnTimeLabel.Text = $"Respawn in {respawnTimeLeft}";
				RespawnTimeLabel.SetClass( "hidden", false );
			}
			else
			{
				RespawnTimeLabel.SetClass( "hidden", true );
			}

			base.Tick();
		}
	}
}
