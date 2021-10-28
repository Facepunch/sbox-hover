using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class HeavyEnergyBoosterConfig : WeaponConfig
	{
		public override string Name => "Energy++";
		public override string Description => "+30 Max Energy";
		public override string SecondaryDescription => "Converts Incoming Damage to Energy";
		public override string Icon => "ui/equipment/heavy_energy_booster.png";
		public override string ClassName => "hv_heavy_energy_booster";
	}

	[Library( "hv_heavy_energy_booster", Title = "Energy++" )]
	public partial class HeavyEnergyBooster : Equipment
	{
		public override WeaponConfig Config => new HeavyEnergyBoosterConfig();
		public override bool IsPassive => true;

		public override DamageInfo OwnerTakeDamage( DamageInfo info )
		{
			if ( Owner is Player player && player.Controller is MoveController controller )
			{
				controller.Energy = (controller.Energy + info.Damage * 0.2f).Clamp( 0f, controller.MaxEnergy );
			}

			return info;
		}

		protected override void OnEquipmentGiven( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.MaxEnergy += 30f;
			}
		}

		protected override void OnEquipmentTaken( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.MaxEnergy -= 30f;
			}
		}
	}
}
