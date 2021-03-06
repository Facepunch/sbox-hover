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
		public override string ModelName => "models/radar_jammer/radar_jammer.vmdl";

		public override void Spawn()
		{
			MaxHealth = 400f;

			base.Spawn();
		}
	}
}
