using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class RadarRangeUpgrade : DependencyUpgrade
	{
		public override string Name => "Range +10%";
		public override string Description => "Upgrade the sensor range";
		public override int TokenCost => 250;

		public override void Apply( GeneratorDependency dependency )
		{
			if ( dependency is RadarSensorEntity sensor)
			{
				sensor.Range *= 1.1f;
			}
		}
	}
}
