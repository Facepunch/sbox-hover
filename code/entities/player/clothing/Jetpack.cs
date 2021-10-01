using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class Jetpack : BaseClothing
	{
		protected Particles Trail { get; set; }

		public override void Spawn()
		{
			SetModel( "models/tempmodels/jetpack/jetpack.vmdl" );

			base.Spawn();
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( !Wearer.IsValid() ) return;

			if ( Wearer.Controller is not MoveController controller )
				return;

			if ( controller.IsJetpacking && Wearer.LifeState == LifeState.Alive )
			{
				if ( Trail == null )
				{
					Trail = Particles.Create( "particles/jetpack/jetpack_trail.vpcf", this, "trail" );
				}
			}
			else if ( Trail != null )
			{
				Trail.Destroy();
				Trail = null;
			}
		}
	}
}
