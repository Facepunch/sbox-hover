using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class FusionAmmoUpgrade : WeaponUpgrade
	{
		public override string Name => "Ammo Pack";
		public override string Description => "+25 Ammo";
		public override string Icon => "ui/icons/icon_upgrade.png";
		public override int TokenCost => 400;

		public override void Restock( Player player, Weapon weapon )
		{
			player.GiveAmmo( weapon.Config.AmmoType, 25 );
		}

		public override void Apply( Player player, Weapon weapon )
		{
			Restock( player, weapon );
		}
	}
}
