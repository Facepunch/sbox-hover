using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class DamageVsHeavy : WeaponUpgrade
	{
		public override string Name => "Armor Piercing";
		public override string Description => "+15% Damage vs Heavy";
		public override string Icon => "ui/icons/icon_upgrade.png";
		public override int TokenCost => 700;

		public override DamageInfo DealDamage( HoverPlayer player, HoverPlayer victim, Weapon weapon, DamageInfo info )
		{
			if ( victim.Loadout.ArmorType == LoadoutArmorType.Heavy )
			{
				info.Damage *= 1.15f;
			}

			return info;
		}
	}
}
