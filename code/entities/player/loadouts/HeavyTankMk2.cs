﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyTankMk2 : HeavyTank
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster";
		public override string Name => "Tank Mk. II";
		public override Type UpgradesTo => null;
		public override int Level => 2;
		public override int UpgradeCost => 750;
		public override float RegenDelay => 15f;
		public override float Health => 2100f;
		public override float Energy => 70f;
	}
}
