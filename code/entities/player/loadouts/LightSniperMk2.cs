using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightSniperMk2 : LightSniper
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster";
		public override string Name => "Sniper Mk. II";
		public override Type UpgradesTo => null;
		public override int UpgradeCost => 500;
		public override float RegenDelay => 15f;
		public override float Health => 800f;
		public override float Energy => 100f;
	}
}
