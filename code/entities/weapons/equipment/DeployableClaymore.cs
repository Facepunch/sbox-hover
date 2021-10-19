using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public class DeployableClaymoreConfig : WeaponConfig
	{
		public override string Name => "Claymore";
		public override string Description => "Directional Explosive Mine";
		public override string Icon => "ui/equipment/claymore.png";
		public override string ClassName => "hv_deployable_claymore";
	}

	[Library( "hv_deployable_claymore", Title = "Claymore" )]
	public partial class DeployableClaymore : DeployableEquipment<Claymore>
	{
		public override WeaponConfig Config => new DeployableClaymoreConfig();
		public override string Model => "models/claymore_mines/claymore_mines.vmdl";
	}
}
