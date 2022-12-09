using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class MotionAlarmRangeUpgrade : WeaponUpgrade
	{
		public override string Name => "Increased Range";
		public override string Description => "+15% Range";
		public override string Icon => "ui/icons/icon_upgrade.png";
		public override int TokenCost => 300;

		public override void Restock( HoverPlayer player, Weapon weapon )
		{

		}

		public override void Apply( HoverPlayer player, Weapon weapon )
		{
			if ( weapon is DeployableMotionAlarm deployable )
			{
				deployable.Radius *= 1.15f;
			}
		}
	}
}
