using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_jump_mine" )]
	public partial class JumpMine : DeployableEntity, IKillFeedIcon
	{
		public override string Model => "models/mines/mines.vmdl";
		public override float MaxHealth => 80f;

		public string GetKillFeedIcon()
		{
			return "ui/killicons/jump_mine.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Jump Mine";
		}
	}
}
