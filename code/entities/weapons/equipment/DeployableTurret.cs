using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public class DeployableTurretConfig : WeaponConfig
	{
		public override string Name => "Turret";
		public override string Description => "Deployable Light Turret";
		public override string Icon => "ui/equipment/deployable_turret.png";
		public override string ClassName => "hv_deployable_turret";
	}

	[Library( "hv_deployable_turret", Title = "Turret" )]
	public partial class DeployableTurret : DeployableEquipment<LightTurret>
	{
		public override WeaponConfig Config => new DeployableTurretConfig();
		public override string Model => "models/deploy_turret/deploy_turret.vmdl";
	}
}
