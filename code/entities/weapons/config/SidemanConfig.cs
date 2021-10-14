using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class SidemanConfig : WeaponConfig
	{
		public override string Name => "Sideman";
		public override string Description => "Short-range hitscan pistol";
		public override string ClassName => "hv_sideman";
		public override string Icon => "ui/weapons/sideman.png";
		public override AmmoType AmmoType => AmmoType.SMG;
		public override int Ammo => 60;
	}
}
