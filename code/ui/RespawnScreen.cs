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
		public Label WeaponName { get; private set; }

		public RespawnKillerInfo()
		{
			KillerAvatar = Add.Image( "", "avatar" );
			KillerName = Add.Label( "", "name" );
			WeaponName = Add.Label( "", "weapon" );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}

		public void Update( Player player )
		{
			KillerAvatar.SetTexture( $"avatar:{player.Client.PlayerId}" );
			KillerName.Text = player.Client.Name;
			KillerName.Style.FontColor = player.Team.GetColor();
			KillerName.Style.Dirty();
		}

		public void Update( IKillFeedIcon killer )
		{
			KillerAvatar.Texture = Texture.Load( killer.GetKillFeedIcon() );
			KillerName.Text = killer.GetKillFeedName();
			KillerName.Style.FontColor = killer.GetKillFeedTeam().GetColor();
			KillerName.Style.Dirty();
		}

		public void Update( string killerName )
		{
			KillerAvatar.Texture = Texture.Load( "ui/icons/skull.png" );
			KillerName.Text = killerName;
			KillerName.Style.FontColor = Color.White;
			KillerName.Style.Dirty();
		}

		public void SetWeapon( Entity weapon )
		{
			if ( weapon.IsValid() )
			{
				SetClass( "has-weapon", true );

				if ( weapon is IKillFeedIcon icon )
					WeaponName.Text = icon.GetKillFeedName();
				else
					WeaponName.Text = weapon.Name;
			}
			else
			{
				SetClass( "has-weapon", false );
			}
		}
	}

	[UseTemplate] 
	public partial class RespawnScreen : Panel
	{
		public static RespawnScreen Instance { get; private set; }

		public RespawnKillerInfo KillerInfo { get; private set; }
		public Label RespawnTimeLabel { get; private set; }
		public Label KilledByLabel { get; private set; }

		public RealTimeUntil RespawnTime { get; private set; }

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
			StyleSheet.Load( "/ui/RespawnScreen.scss" );

			//RespawnTimeLabel = Add.Label( "", "respawn" );
			//KilledByLabel = Add.Label( "You were killed by", "killedby" );
			//KillerInfo = AddChild<RespawnKillerInfo>( "killer" );

			SetClass( "hidden", true );

			Instance = this;
		}

		public override void Tick()
		{
			var respawnTimeLeft = RespawnTime.Relative.CeilToInt();

			if ( respawnTimeLeft > 0 )
			{
				RespawnTimeLabel.Text = $"{respawnTimeLeft}s";
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
