using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class MaxDeployableUpgrade : WeaponUpgrade
	{
		public override string Name => "Additional Deployment";
		public override string Description => "+1 Deployment";
		public override string Icon => "ui/icons/icon_upgrade.png";
		public override int TokenCost => 500;

		public override void Restock( HoverPlayer player, Weapon weapon )
		{

		}

		public override void Apply( HoverPlayer player, Weapon weapon )
		{
			if ( weapon is IDeployableEquipment equipment )
			{
				equipment.MaxDeployables++;
			}
		}
	}
}
