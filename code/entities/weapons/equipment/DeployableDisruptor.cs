using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public class DeployableDisruptorConfig : WeaponConfig
	{
		public override string Name => "Disruptor";
		public override string Description => "Deployable Disruptor";
		public override string SecondaryDescription => "Reveals Stealth and Disrupts Radar";
		public override string Icon => "ui/equipment/deployable_disruptor.png";
		public override string ClassName => "hv_deployable_disruptor";
		public override List<Type> Upgrades => new()
		{
			typeof( MaxDeployableUpgrade ),
			typeof( MaxDeployableUpgrade )
		};
		public override WeaponType Type => WeaponType.Deployable;
	}

	[Library( "hv_deployable_disruptor", Title = "Disruptor" )]
	public partial class DeployableDisruptor : DeployableEquipment<Disruptor>
	{
		public override WeaponConfig Config => new DeployableDisruptorConfig();
		public override string Model => "models/radar_jammer/radar_jammer.vmdl";
		public override float DeployScale => 0.1f;
	}
}
