using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public class DeployableForceShieldConfig : WeaponConfig
	{
		public override string Name => "Force Shield";
		public override string Description => "Deployable Force Shield";
		public override string SecondaryDescription => "Blocks Projectiles and Damages Moving Enemies";
		public override string Icon => "ui/equipment/force_shield.png";
		public override string ClassName => "hv_deployable_force_shield";
		public override List<Type> Upgrades => new()
		{
			typeof( ForceShieldHealthUpgrade ),
			typeof( MaxDeployableUpgrade ),
			typeof( ForceShieldHealthUpgrade )
		};
	}

	[Library( "hv_deployable_force_shield", Title = "Force Shield" )]
	public partial class DeployableForceShield : DeployableEquipment<ForceShield>
	{
		public override WeaponConfig Config => new DeployableForceShieldConfig();
		public override string Model => "models/force_field/force_field.vmdl";

		public float ShieldHealth { get; set; } = 1250f;

		protected override void OnDeploy( ForceShield deployable )
		{
			deployable.MaxHealth = ShieldHealth;

			base.OnDeploy( deployable );
		}
	}
}
