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

		private Team CurrentTeam { get; set; }

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}

		public void Update( Player player )
		{
			KillerAvatar.SetTexture( $"avatar:{player.Client.PlayerId}" );
			KillerName.Text = player.Client.Name;
			SetTeam( player.Team );
		}

		public void Update( IKillFeedIcon killer )
		{
			KillerAvatar.Texture = Texture.Load( killer.GetKillFeedIcon() );
			KillerName.Text = killer.GetKillFeedName();
			SetTeam( killer.GetKillFeedTeam() );
		}

		public void Update( string killerName )
		{
			KillerAvatar.Texture = Texture.Load( "ui/icons/skull.png" );
			KillerName.Text = killerName;
			SetTeam( Team.None );
		}

		public void SetTeam( Team team )
		{
			SetClass( CurrentTeam.GetHudClass(), false );
			SetClass( team.GetHudClass(), true );
			CurrentTeam = team;
		}

		public void SetWeapon( Entity weapon )
		{
			if ( weapon.IsValid() && !weapon.IsWorld )
			{
				SetClass( "has-weapon", true );

				if ( weapon is IKillFeedIcon icon )
					WeaponName.Text = icon.GetKillFeedName();
				else if ( weapon is Weapon castWeapon )
					WeaponName.Text = castWeapon.Config.Name;
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
