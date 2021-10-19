using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
    public partial class ForceShieldHealthUpgrade : WeaponUpgrade
	{
		public override string Name => "Increased Health";
		public override string Description => "+300 Health";
		public override int TokenCost => 500;

		public override void Apply( Player player, Weapon weapon )
		{
			if ( weapon is DeployableForceShield deployable )
			{
				deployable.ShieldHealth += 350f;
			}
		}
	}
}
