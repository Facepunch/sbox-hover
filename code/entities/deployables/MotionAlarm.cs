using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_motion_alarm" )]
	public partial class MotionAlarm : DeployableEntity
	{
		public override string Model => "models/motion_sensor/motion_sensor.vmdl";
		public override float MaxHealth => 300f;
		public float Radius { get; set; } = 300f;

		public string GetKillFeedIcon()
		{
			return "ui/killicons/motion_alarm.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Motion Alarm";
		}
	}
}
