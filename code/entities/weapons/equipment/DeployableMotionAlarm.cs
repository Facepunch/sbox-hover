using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public class DeployableMotionAlarmConfig : WeaponConfig
	{
		public override string Name => "Motion Alarm";
		public override string Description => "Detects and Slows Enemy Targets";
		public override string SecondaryDescription => "-20% Energy to Spotted Targets";
		public override string Icon => "ui/equipment/motion_alarm.png";
		public override string ClassName => "hv_deployable_motion_alarm";
		public override List<Type> Upgrades => new()
		{
			typeof( MaxDeployableUpgrade ),
			typeof( MotionAlarmRangeUpgrade ),
			typeof( MaxDeployableUpgrade ),
		};
		public override WeaponType Type => WeaponType.Deployable;
	}

	[Library( "hv_deployable_motion_alarm", Title = "Motion Alarm" )]
	public partial class DeployableMotionAlarm : DeployableEquipment<MotionAlarm>
	{
		public override WeaponConfig Config => new DeployableMotionAlarmConfig();
		public override string Model => "models/motion_sensor/motion_sensor.vmdl";

		public float Radius { get; set; } = 300f;

		protected override void OnDeploy( MotionAlarm deployable )
		{
			deployable.Radius = Radius;

			base.OnDeploy( deployable );
		}
	}
}
