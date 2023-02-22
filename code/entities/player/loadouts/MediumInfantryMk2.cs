using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class MediumInfantryMk2 : MediumInfantry
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster";
		public override string Name => "Infantry Mk. II";
		public override Type UpgradesTo => null;
		public override int Level => 2;
		public override int UpgradeCost => 600;
		public override float RegenDelay => 15f;
		public override float Health => 1300f;
		public override float Energy => 90f;
	}
}
