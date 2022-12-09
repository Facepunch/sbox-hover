using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class HeavyEnergyBoosterConfig : WeaponConfig
	{
		public override string Name => "Energy++";
		public override string Description => "+30 Max Energy";
		public override string SecondaryDescription => "Converts Incoming Damage to Energy";
		public override string Icon => "ui/equipment/heavy_energy_booster.png";
		public override string ClassName => "hv_heavy_energy_booster";
		public override WeaponType Type => WeaponType.Equipment;
	}

	[Library( "hv_heavy_energy_booster", Title = "Energy++" )]
	public partial class HeavyEnergyBooster : Equipment
	{
		public override WeaponConfig Config => new HeavyEnergyBoosterConfig();
		public override bool IsPassive => true;

		public override DamageInfo OwnerTakeDamage( DamageInfo info )
		{
			if ( Owner is HoverPlayer player )
			{
				player.Energy = (player.Energy + info.Damage * 0.2f).Clamp( 0f, player.MaxEnergy );
			}

			return info;
		}

		protected override void OnEquipmentGiven( HoverPlayer player )
		{
			player.MaxEnergy += 30f;
		}

		protected override void OnEquipmentTaken( HoverPlayer player )
		{
			player.MaxEnergy -= 30f;
		}
	}
}
