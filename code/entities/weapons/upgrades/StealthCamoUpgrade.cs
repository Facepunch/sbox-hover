using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class StealthCamoUpgrade : WeaponUpgrade
	{
		public override string Name => "Stealth Capacitor";
		public override string Description => "-10% Stealth Energy Drain";
		public override int TokenCost => 500;

		public override void Apply( Player player, Weapon weapon )
		{
			if ( weapon is StealthCamo camo )
			{
				camo.EnergyDrain *= 0.9f;
			}
		}
	}
}
