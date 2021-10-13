using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class HeavyShieldBoosterConfig : WeaponConfig
	{
		public override string Name => "Shield++";
		public override string Description => "-10% Energy Regen and +10% Move Shield";
		public override string Icon => "ui/equipment/heavy_shield_booster.png";
		public override string ClassName => "hv_heavy_shield_booster";
	}

	[Library( "hv_heavy_shield_booster", Title = "Shield+" )]
	public partial class HeavyShieldBooster : Equipment
	{
		public override WeaponConfig Config => new HeavyShieldBoosterConfig();
		public override bool CanSelectWeapon => false;

		protected override void OnEquipmentGiven( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.JetpackGainPerSecond *= 0.1f;
			}
		}

		protected override void OnEquipmentTaken( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.JetpackGainPerSecond *= 1f / 0.9f;
			}
		}
	}
}
