using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class RadarJammerConfig : WeaponConfig
	{
		public override string Name => "Jammer";
		public override string Description => "Invisible to Radar Sensors";
		public override string Icon => "ui/equipment/radar_jammer.png";
		public override string ClassName => "hv_radar_jammer";
	}

	[Library( "hv_radar_jammer", Title = "Jammer" )]
	public partial class RadarJammer : Equipment
	{
		public override WeaponConfig Config => new RadarJammerConfig();
		public override bool IsPassive => true;
	}
}
