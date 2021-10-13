using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class AmmoPackUpgrade : WeaponUpgrade
	{
		public override string Name => "Ammo Pack";
		public override string Description => "+1 Ammo Clip";
		public override int TokenCost => 500;

		public override void Restock( Player player, Weapon weapon )
		{
			player.GiveAmmo( weapon.Config.AmmoType, weapon.AmmoClip );
		}

		public override void Apply( Player player, Weapon weapon )
		{
			Restock( player, weapon );
		}
	}
}
