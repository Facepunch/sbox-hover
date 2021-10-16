using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public abstract partial class DependencyUpgrade
	{
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual int TokenCost => 1000;

		public virtual void Apply( GeneratorDependency dependency )
		{

		}
	}
}
