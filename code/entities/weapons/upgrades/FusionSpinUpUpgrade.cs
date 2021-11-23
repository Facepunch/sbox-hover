using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class FusionSpinUpUpgrade : WeaponUpgrade
	{
		public override string Name => "Decreased Spin Time";
		public override string Description => "-15% Spin Time";
		public override string Icon => "ui/icons/upgrade.png";
		public override int TokenCost => 400;

		public override void Apply( Player player, Weapon weapon )
		{
			if ( weapon is Fusion fusion )
			{
				fusion.SpinUpTime *= 0.85f;
			}
		}
	}
}
