using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightSaboteurMk2 : LightSaboteur
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster";
		public override string Name => "Saboteur Mk. II";
		public override Type UpgradesTo => null;
		public override int Level => 2;
		public override int UpgradeCost => 500;
		public override float RegenDelay => 15f;
		public override float Health => 800f;
		public override float Energy => 100f;
	}
}
