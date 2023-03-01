using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class StickyGrenade : Projectile
	{
		private bool IsPrimedToExplode { get; set; }
		private Entity AttachedTo { get; set; }

		public override void Simulate()
		{
			if ( !IsPrimedToExplode && DestroyTime < 1f )
			{
				IsPrimedToExplode = true;
				PlaySound( "sticky.warning" );
			}

			if ( AttachedTo.IsValid() )
			{
				if ( DestroyTime )
				{
					PlayHitEffects( Vector3.Zero );
					Callback?.Invoke( this, AttachedTo );
					Delete();
				}
			}
			else
			{
				base.Simulate();
			}
		}

		protected override bool HasHitTarget( TraceResult trace )
		{
			if ( trace.Entity.IsValid() )
			{
				AttachedTo = trace.Entity;
				SetParent( trace.Entity );
				PlaySound( "sticky.attach" );
				DestroyTime = 3f;
			}

			return false;
		}
	}
}
