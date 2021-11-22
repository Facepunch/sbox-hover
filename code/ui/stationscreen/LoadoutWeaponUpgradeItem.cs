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
		public Panel Icon { get; private set; }
		public int Index { get; private set; }

		public LoadoutWeaponUpgradeItem()
		{
			BindClass( "owned", () => IsOwned() );
			BindClass( "locked", () => IsLocked() );
		}

		public bool IsOwned()
		{
			if ( Local.Pawn is not Player player )
				return true;

			var ownedUpgrades = player.GetWeaponUpgrades( Weapon );
			return (ownedUpgrades != null && ownedUpgrades.Count > Index);
		}

		public bool IsLocked()
		{
			if ( Local.Pawn is not Player player )
				return true;

			if ( Index == 0 )
				return false;

			var ownedUpgrades = player.GetWeaponUpgrades( Weapon );
			return (ownedUpgrades == null || ownedUpgrades.Count < Index);
		}

		public void SetUpgrade( int index, WeaponConfig weapon, WeaponUpgrade upgrade )
		{
			Weapon = weapon;
			Upgrade = upgrade;
			Index = index;
		}

		public void DoBuyUpgrade()
		{
			if ( !IsLocked() && !IsOwned() )
			{
				Player.BuyWeaponUpgrade( Weapon.GetType().Name, Upgrade.GetType().Name );
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
