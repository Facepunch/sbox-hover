using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class HeavyEnergyBoosterConfig : WeaponConfig
	{
		public override string Name => "Energy++";
		public override string Description => "+25 Max Energy + 15% Energy Regen";
		public override string Icon => "ui/equipment/heavy_energy_booster.png";
		public override string ClassName => "hv_heavy_energy_booster";
	}

	[Library( "hv_heavy_energy_booster", Title = "Energy++" )]
	public partial class HeavyEnergyBooster : Equipment
	{
		public override WeaponConfig Config => new HeavyEnergyBoosterConfig();
		public override bool CanSelectWeapon => false;

		protected override void OnEquipmentGiven( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.JetpackGainPerSecond *= 1.15f;
				controller.MaxEnergy += 25f;
			}
		}

		protected override void OnEquipmentTaken( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.JetpackGainPerSecond *= 1f / 1.15f;
				controller.MaxEnergy -= 25f;
			}
		}
	}
}
