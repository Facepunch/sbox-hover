using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public class RespawnKillerInfo : Panel
	{
		public Image KillerAvatar { get; private set; }
		public Label KillerName { get; private set; }
		public Label WeaponName { get; private set; }

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}

		public void Update( Player player )
		{
			KillerAvatar.SetTexture( $"avatar:{player.Client.PlayerId}" );
			KillerName.Text = player.Client.Name;

		}

		public void Update( IKillFeedIcon killer )
		{
			KillerAvatar.Texture = Texture.Load( killer.GetKillFeedIcon() );
			KillerName.Text = killer.GetKillFeedName();
		}

		public void Update( string killerName )
		{
			KillerAvatar.Texture = Texture.Load( "ui/icons/skull.png" );
			KillerName.Text = killerName;
		}

		public void SetWeapon( Entity weapon )
		{
			if ( weapon.IsValid() && !weapon.IsWorld )
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
}
