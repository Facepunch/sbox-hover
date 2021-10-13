using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightAssaultMk2 : LightAssault
	{
		public override string SecondaryDescription => "Has more health and energy and regenerates faster.";
		public override string Description => "A fast assault unit with medium health and high energy.";
		public override string Name => "Light Assault Mk. II";
		public override Type UpgradesTo => null;
		public override int UpgradeCost => 1000;
		public override int TokenCost => 300;
		public override float RegenDelay => 15f;
		public override float Health => 600f;
		public override float Energy => 110f;
		public override float MoveSpeed => 450f;
		public override float MaxSpeed => 1600f;
	}
}
