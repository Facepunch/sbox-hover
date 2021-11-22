using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public class DeployableJumpMineConfig : WeaponConfig
	{
		public override string Name => "Jump Mine";
		public override string Description => "Radial Explosive Mine";
		public override string Icon => "ui/equipment/jump_mine.png";
		public override string ClassName => "hv_deployable_jump_mine";
		public override List<Type> Upgrades => new()
		{
			typeof( MaxDeployableUpgrade ),
			typeof( JumpMineRangeUpgrade ),
			typeof( DeployableDamageVsHeavy ),
		};
	}

	[Library( "hv_deployable_jump_mine", Title = "Jump Mine" )]
	public partial class DeployableJumpMine : DeployableEquipment<JumpMine>, IDeployableDamageVsHeavy
	{
		public override WeaponConfig Config => new DeployableJumpMineConfig();
		public override string Model => "models/mines/mines.vmdl";

		public float DamageVsHeavy { get; set; } = 1f;
		public float Radius { get; set; } = 150f;

		protected override void OnDeploy( JumpMine deployable )
		{
			deployable.Radius = Radius;

			base.OnDeploy( deployable );
		}
	}
}
