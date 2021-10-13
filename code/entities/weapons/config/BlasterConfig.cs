using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class BlasterConfig : WeaponConfig
	{
		public override string Name => "Blaster";
		public override string Description => "Medium-range projectile plasma SMG.";
		public override string Icon => "ui/weapons/blaster.png";
		public override string ClassName => "hv_blaster";
		public override AmmoType AmmoType => AmmoType.SMG;
		public override int Ammo => 90;
	}
}
