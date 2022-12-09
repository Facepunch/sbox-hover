using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class SpeedBoosterConfig : WeaponConfig
	{
		public override string Name => "Speed+";
		public override string Description => "+20% Max Speed and +10% Move Speed";
		public override string Icon => "ui/equipment/speed_booster.png";
		public override string ClassName => "hv_speed_booster";
		public override WeaponType Type => WeaponType.Equipment;
	}

	[Library( "hv_speed_booster", Title = "Speed+" )]
	public partial class SpeedBooster : Equipment
	{
		public override WeaponConfig Config => new SpeedBoosterConfig();
		public override bool IsPassive => true;

		protected override void OnEquipmentGiven( HoverPlayer player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.MoveSpeed *= 1.1f;
				controller.MaxSpeed *= 1.2f;
			}
		}

		protected override void OnEquipmentTaken( HoverPlayer player )
		{
			if ( player.Controller is MoveController controller )
			{
				controller.MoveSpeed *= 1f / 1.1f;
				controller.MaxSpeed *= 1f / 1.2f;
			}
		}
	}
}
