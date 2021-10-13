using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class EnergyBoosterConfig : WeaponConfig
	{
		public override string Name => "Energy+";
		public override string Description => "+20 Max Energy and +10% Energy Regen";
		public override string Icon => "ui/equipment/energy_booster.png";
		public override string ClassName => "hv_energy_booster";
	}

	[Library( "hv_energy_booster", Title = "Energy+" )]
	public partial class EnergyBooster : Equipment
	{
		public override WeaponConfig Config => new EnergyBoosterConfig();
		public override bool CanSelectWeapon => false;

		protected override void OnEquipmentGiven( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.JetpackGainPerSecond *= 1.1f;
				controller.MaxEnergy += 20f;
			}
		}

		protected override void OnEquipmentTaken( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.JetpackGainPerSecond *= 1f / 1.1f;
				controller.MaxEnergy -= 20f;
			}
		}
	}
}
