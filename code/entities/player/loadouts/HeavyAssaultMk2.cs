﻿using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyAssaultMk2 : HeavyAssault
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster";
		public override string Name => "Heavy Assault Mk. II";
		public override Type UpgradesTo => null;
		public override int UpgradeCost => 1000;
		public override int TokenCost => 1000;
		public override float RegenDelay => 15f;
		public override float Health => 1200f;
		public override float Energy => 80f;
	}
}