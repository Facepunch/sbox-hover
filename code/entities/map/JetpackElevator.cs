using Sandbox;
using Editor;

namespace Facepunch.Hover
{
	[Library( "hv_jetpack_elevator" )]
	[AutoApplyMaterial( "materials/editor/hv_jetpack_elevator.vmat" )]
	[Solid]
	[HammerEntity]
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
