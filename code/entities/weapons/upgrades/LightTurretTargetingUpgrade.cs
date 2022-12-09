using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class LightTurretTargetingUpgrade : WeaponUpgrade
	{
		public override string Name => "Increased Targeting Speed";
		public override string Description => "+15% Targeting Speed";
		public override string Icon => "ui/icons/icon_upgrade.png";
		public override int TokenCost => 400;

		public override void Restock( HoverPlayer player, Weapon weapon )
		{

		}

		public override void Apply( HoverPlayer player, Weapon weapon )
		{
			if ( weapon is DeployableTurret deployable )
			{
				deployable.TargetingSpeed *= 0.85f;
			}
		}
	}
}
