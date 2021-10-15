using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightSaboteurMk2 : LightSaboteur
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster";
		public override string Name => "Light Saboteur Mk. II";
		public override Type UpgradesTo => null;
		public override int UpgradeCost => 600;
		public override int TokenCost => 300;
		public override float RegenDelay => 15f;
		public override float Health => 600f;
		public override float Energy => 100f;
	}
}
