using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class TurretTargetingUpgrade : DependencyUpgrade
	{
		public override string Name => "Targeting Speed +15%";
		public override string Description => "Upgrade the targeting speed";
		public override int TokenCost => 1500;

		public override void Apply( GeneratorDependency dependency )
		{
			if ( dependency is TurretAsset turret )
			{
				turret.TargetingSpeed *= 0.85f;
			}
		}
	}
}
