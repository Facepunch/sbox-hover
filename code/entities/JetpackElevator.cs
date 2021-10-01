using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_jetpack_elevator" )]
	[Hammer.AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
	[Hammer.Solid]
	public partial class JetpackElevator : BaseTrigger
	{
		public override void Spawn()
		{
			base.Spawn();

			EnableDrawing = false;
			Transmit = TransmitType.Always;
		}
	}
}
