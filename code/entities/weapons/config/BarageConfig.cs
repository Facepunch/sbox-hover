using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class BarageConfig : WeaponConfig
	{
		public override string Name => "Barage";
		public override string Description => "Short-range grenade launcher.";
		public override string Icon => "ui/weapons/blaster.png";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override int Ammo => 20;
	}
}
