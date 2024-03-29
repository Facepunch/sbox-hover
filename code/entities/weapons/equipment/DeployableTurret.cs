﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public class DeployableTurretConfig : WeaponConfig
	{
		public override string Name => "Turret";
		public override string Description => "Deployable Light Turret";
		public override string Icon => "ui/equipment/deployable_turret.png";
		public override string ClassName => "hv_deployable_turret";
		public override List<Type> Upgrades => new()
		{
			typeof( LightTurretTargetingUpgrade ),
			typeof( MaxDeployableUpgrade ),
			typeof( DeployableDamageVsHeavy ),
		};
		public override WeaponType Type => WeaponType.Deployable;
	}

	[Library( "hv_deployable_turret", Title = "Turret" )]
	public partial class DeployableTurret : DeployableEquipment<LightTurret>, IDeployableDamageVsHeavy
	{
		public override WeaponConfig Config => new DeployableTurretConfig();
		public override string ModelName => "models/deploy_turret/deploy_turret.vmdl";
		public override int MaxDeployables { get; set; } = 2;

		public float TargetingSpeed { get; set; } = 1f;
		public float DamageVsHeavy { get; set; } = 1f;

		protected override void OnDeploy( LightTurret deployable )
		{
			deployable.TargetingSpeed = TargetingSpeed;
			deployable.DamageVsHeavy = DamageVsHeavy;

			base.OnDeploy( deployable );
		}
	}
}
