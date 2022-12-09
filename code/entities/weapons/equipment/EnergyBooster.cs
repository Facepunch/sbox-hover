using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class EnergyBoosterConfig : WeaponConfig
	{
		public override string Name => "Energy+";
		public override string Description => "+20 Max Energy and +10% Energy Regen";
		public override string Icon => "ui/equipment/energy_booster.png";
		public override string ClassName => "hv_energy_booster";
		public override WeaponType Type => WeaponType.Equipment;
	}

	[Library( "hv_energy_booster", Title = "Energy+" )]
	public partial class EnergyBooster : Equipment
	{
		public override WeaponConfig Config => new EnergyBoosterConfig();
		public override bool IsPassive => true;

		protected override void OnEquipmentGiven( HoverPlayer player )
		{
			player.EnergyRegen *= 1.1f;
			player.MaxEnergy += 20f;
		}

		protected override void OnEquipmentTaken( HoverPlayer player )
		{
			player.EnergyRegen *= 1f / 1.1f;
			player.MaxEnergy -= 20f;
		}
	}
}
