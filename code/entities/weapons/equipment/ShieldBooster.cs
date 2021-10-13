using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class ShieldBoosterConfig : WeaponConfig
	{
		public override string Name => "Shield+";
		public override string Description => "-20% Energy Regen and +10% Move Shield";
		public override string Icon => "ui/equipment/shield_booster.png";
		public override string ClassName => "hv_shield_booster";
	}

	[Library( "hv_shield_booster", Title = "Shield+" )]
	public partial class ShieldBooster : Equipment
	{
		public override WeaponConfig Config => new ShieldBoosterConfig();
		public override bool CanSelectWeapon => false;

		protected override void OnEquipmentGiven( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.JetpackGainPerSecond *= 0.8f;
			}
		}

		protected override void OnEquipmentTaken( Player player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.JetpackGainPerSecond *= 1f / 0.8f;
			}
		}
	}
}
