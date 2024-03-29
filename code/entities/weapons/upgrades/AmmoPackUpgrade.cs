﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class AmmoPackUpgrade : WeaponUpgrade
	{
		public override string Name => "Ammo Pack";
		public override string Description => "+1 Ammo Clip";
		public override string Icon => "ui/icons/icon_upgrade.png";
		public override int TokenCost => 500;

		public override void Restock( HoverPlayer player, Weapon weapon )
		{
			player.GiveAmmo( weapon.Config.AmmoType, weapon.AmmoClip );
		}

		public override void Apply( HoverPlayer player, Weapon weapon )
		{
			Restock( player, weapon );
		}
	}
}
