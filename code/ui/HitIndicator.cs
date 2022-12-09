using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Facepunch.Hover.UI
{
	[StyleSheet( "/ui/HitIndicator.scss" )]
	public partial class HitIndicator : Panel
	{
		public static HitIndicator Current { get; private set; }

		public HitIndicator()
		{
			Current = this;
		}

		public override void Tick()
		{
			base.Tick();
		}

		public void OnHit( Vector3 position, float amount )
		{
			_ = new HitPoint( amount, position, this );
		}

		public class HitPoint : Panel
		{
			public HitPoint( float amount, Vector3 position, Panel parent )
			{
				Parent = parent;
				_ = Lifetime();
			}

			async Task Lifetime()
			{
				await Task.Delay( 200 );
				Delete();
			}
		}
	}
}
