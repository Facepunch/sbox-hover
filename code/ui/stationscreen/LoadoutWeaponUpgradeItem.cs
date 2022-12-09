using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class LoadoutWeaponUpgradeItem : Panel
	{
		public string ButtonText => GetButtonText();
		public StationScreenButton BuyButton { get; private set; }
		public WeaponUpgrade Upgrade { get; private set; }
		public WeaponConfig Weapon { get; private set; }
		public Image Icon { get; private set; }
		public int Index { get; private set; }

		public LoadoutWeaponUpgradeItem()
		{
			BindClass( "owned", () => IsOwned() );
			BindClass( "locked", () => IsLocked() );
		}

		public bool IsOwned()
		{
			if ( Local.Pawn is not HoverPlayer player )
				return true;

			var ownedUpgrades = player.GetWeaponUpgrades( Weapon );
			return (ownedUpgrades != null && ownedUpgrades.Count > Index);
		}

		public bool IsLocked()
		{
			if ( Local.Pawn is not HoverPlayer player )
				return true;

			if ( Index == 0 )
				return false;

			var ownedUpgrades = player.GetWeaponUpgrades( Weapon );
			return (ownedUpgrades == null || ownedUpgrades.Count < Index);
		}

		public void SetUpgrade( int index, WeaponConfig weapon, WeaponUpgrade upgrade )
		{
			Icon.SetTexture( upgrade.Icon );
			Weapon = weapon;
			Upgrade = upgrade;
			Index = index;
		}

		public void DoBuyUpgrade()
		{
			if ( Local.Pawn is not HoverPlayer player )
				return;

			if ( !IsLocked() && !IsOwned() )
			{
				if ( player.HasTokens( Upgrade.TokenCost ) )
				{
					HoverPlayer.BuyWeaponUpgrade( Weapon.GetType().Name, Upgrade.GetType().Name );
				}
				else
				{
					var tokensNeeded = Upgrade.TokenCost - player.Tokens;
					UI.Hud.Toast( $"You need {tokensNeeded} Tokens for this upgrade!", "ui/icons/icon_currency_blue.png" );
				}

				Audio.Play( "hover.clickbeep" );
			}
		}

		public override void Tick()
		{
			BuyButton.SetDisabled( IsLocked() || IsOwned() );

			base.Tick();
		}

		protected string GetButtonText()
		{
			if ( IsLocked() ) return "Locked";
			if ( IsOwned() ) return "Owned";

			return Upgrade.TokenCost.ToString();
		}
	}
}
