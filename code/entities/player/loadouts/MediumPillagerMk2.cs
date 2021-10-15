using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class MediumPillagerMk2 : MediumPillager
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster";
		public override string Name => "Medium Pillager Mk. II";
		public override Type UpgradesTo => null;
		public override int UpgradeCost => 600;
		public override int TokenCost => 300;
		public override float RegenDelay => 15f;
		public override float Health => 1000f;
		public override float Energy => 90f;
	}
}
