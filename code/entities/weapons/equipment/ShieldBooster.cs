using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class ShieldBoosterConfig : WeaponConfig
	{
		public override string Name => "Shield+";
		public override string Description => "-20% Energy Regen and Energy Absorbs Damage";
		public override string Icon => "ui/equipment/shield_booster.png";
		public override string ClassName => "hv_shield_booster";
	}

	[Library( "hv_shield_booster", Title = "Shield+" )]
	public partial class ShieldBooster : Equipment
	{
		public override WeaponConfig Config => new ShieldBoosterConfig();
		public override bool CanSelectWeapon => false;

		public override DamageInfo OwnerTakeDamage( DamageInfo info )
		{
			if ( Owner is Player player && player.Controller is MoveController controller )
			{
				info.Damage = Math.Max( info.Damage - controller.Energy, 0f );
				controller.Energy *= 0.5f;
			}

			return info;
		}

		protected override void OnEquipmentGiven( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.EnergyRegen *= 0.8f;
			}
		}

		protected override void OnEquipmentTaken( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.EnergyRegen *= 1f / 0.8f;
			}
		}
	}
}
