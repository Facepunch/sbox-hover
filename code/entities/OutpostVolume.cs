using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_outpost_volume" )]
	[Hammer.AutoApplyMaterial( "materials/editor/hv_jetpack_elevator.vmat" )]
	[Hammer.Solid]
	public partial class OutpostVolume : BaseTrigger
	{
		[Net, Property] public string OutpostName { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			EnableDrawing = false;
			Transmit = TransmitType.Always;
		}
	}
}
