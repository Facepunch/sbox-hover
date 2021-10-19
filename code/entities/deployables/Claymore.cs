using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_claymore" )]
	public partial class Claymore : DeployableEntity, IKillFeedIcon
	{
		public override string Model => "models/claymore_mines/claymore_mines.vmdl";
		public override float MaxHealth => 80f;

		public string GetKillFeedIcon()
		{
			return "ui/killicons/claymore.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Claymore";
		}
	}
}
