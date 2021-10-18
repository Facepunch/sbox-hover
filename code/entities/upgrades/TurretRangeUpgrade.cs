using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class TurretRangeUpgrade : DependencyUpgrade
	{
		public override string Name => "Range +15%";
		public override string Description => "Upgrade the attack range";
		public override int TokenCost => 1000;

		public override void Apply( GeneratorDependency dependency )
		{
			if ( dependency is TurretEntity turret )
			{
				turret.AttackRadius *= 1.1f;
			}
		}
	}
}
