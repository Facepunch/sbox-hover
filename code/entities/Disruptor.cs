using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_disruptor" )]
	public partial class Disruptor : DeployableEntity
	{
		public override string Model => "models/radar_jammer/radar_jammer.vmdl";
		public override float MaxHealth => 600f;
	}
}
