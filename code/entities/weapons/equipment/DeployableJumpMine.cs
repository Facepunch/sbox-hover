using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public class DeployableJumpMineConfig : WeaponConfig
	{
		public override string Name => "Jump Mine";
		public override string Description => "Radial Explosive Mine";
		public override string Icon => "ui/equipment/jump_mine.png";
		public override string ClassName => "hv_deployable_jump_mine";
	}

	[Library( "hv_deployable_jump_mine", Title = "Jump Mine" )]
	public partial class DeployableJumpMine : DeployableEquipment<JumpMine>
	{
		public override WeaponConfig Config => new DeployableJumpMineConfig();
		public override string Model => "models/mines/mines.vmdl";
	}
}
