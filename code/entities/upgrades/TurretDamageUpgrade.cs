using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class TurretDamageUpgrade : DependencyUpgrade
	{
		public override string Name => "Damage +15%";
		public override string Description => "Upgrade the attack damage";
		public override int TokenCost => 1500;

		public override void Apply( GeneratorDependency dependency )
		{
			if ( dependency is TurretAsset turret )
			{
				turret.BlastDamage *= 1.15f;
			}
		}
	}
}
