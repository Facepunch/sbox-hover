using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class LongshotConfig : WeaponConfig
	{
		public override string Name => "Longshot";
		public override string Description => "Long-range hitscan sniper rifle.";
		public override string Icon => "ui/weapons/blaster.png";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override int Ammo => 20;
	}
}
