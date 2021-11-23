using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public class DeployableClaymoreConfig : WeaponConfig
	{
		public override string Name => "Claymore";
		public override string Description => "Directional Explosive Mine";
		public override string Icon => "ui/equipment/claymore.png";
		public override string ClassName => "hv_deployable_claymore";
		public override List<Type> Upgrades => new()
		{
			typeof( MaxDeployableUpgrade ),
			typeof( ClaymoreRangeUpgrade ),
			typeof( DeployableDamageVsHeavy ),
		};
		public override WeaponType Type => WeaponType.Deployable;
	}

	[Library( "hv_deployable_claymore", Title = "Claymore" )]
	public partial class DeployableClaymore : DeployableEquipment<Claymore>, IDeployableDamageVsHeavy
	{
		public override WeaponConfig Config => new DeployableClaymoreConfig();
		public override string Model => "models/claymore_mines/claymore_mines.vmdl";

		public float DamageVsHeavy { get; set; } = 1f;
		public float Radius { get; set; } = 100f;

		protected override void OnDeploy( Claymore deployable )
		{
			deployable.Radius = Radius;

			base.OnDeploy( deployable );
		}
	}
}
