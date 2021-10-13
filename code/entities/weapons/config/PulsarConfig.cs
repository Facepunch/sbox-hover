using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class PulsarConfig : WeaponConfig
	{
		public override string Name => "Pulsar";
		public override string Description => "Long-range explosive projectile based rifle.";
		public override string Icon => "ui/weapons/pulsar.png";
		public override string ClassName => "hv_pulsar";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override int Ammo => 30;
	}
}
