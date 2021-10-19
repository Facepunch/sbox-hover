﻿using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyGunnerMk2 : HeavyGunner
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster";
		public override string Name => "Heavy Gunner Mk. II";
		public override Type UpgradesTo => null;
		public override int UpgradeCost => 750;
		public override float RegenDelay => 15f;
		public override float Health => 1200f;
		public override float Energy => 80f;
	}
}
