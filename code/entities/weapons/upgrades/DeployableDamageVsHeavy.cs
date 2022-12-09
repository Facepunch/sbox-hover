using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class DeployableDamageVsHeavy : WeaponUpgrade
	{
		public override string Name => "Armor Piercing";
		public override string Description => "+15% Damage vs Heavy";
		public override string Icon => "ui/icons/icon_upgrade.png";
		public override int TokenCost => 700;

		public override void Apply( HoverPlayer player, Weapon weapon )
		{
			if ( weapon is IDeployableDamageVsHeavy equipment )
			{
				equipment.DamageVsHeavy *= 1.15f;
			}
		}
	}
}
