﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class JumpMineRangeUpgrade : WeaponUpgrade
	{
		public override string Name => "Increased Range";
		public override string Description => "+15% Range";
		public override int TokenCost => 300;

		public override void Restock( Player player, Weapon weapon )
		{

		}

		public override void Apply( Player player, Weapon weapon )
		{
			if ( weapon is DeployableJumpMine deployable )
			{
				deployable.Radius *= 1.15f;
			}
		}
	}
}