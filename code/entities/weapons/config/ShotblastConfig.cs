using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class ShotblastConfig : WeaponConfig
	{
		public override string Name => "Shotblast";
		public override string Description => "Short-range hitscan shotgun.";
		public override string Icon => "ui/weapons/shotblast.png";
		public override string ClassName => "hv_shotblast";
		public override AmmoType AmmoType => AmmoType.Shotgun;
		public override int Ammo => 30;
	}
}
