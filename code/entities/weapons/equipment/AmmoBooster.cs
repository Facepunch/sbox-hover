using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class AmmoBoosterConfig : WeaponConfig
	{
		public override string Name => "Ammo+";
		public override string Description => "+30% Ammo";
		public override string Icon => "ui/equipment/ammo_booster.png";
		public override string ClassName => "hv_ammo_booster";
		public override WeaponType Type => WeaponType.Equipment;
	}

	[Library( "hv_ammo_booster", Title = "Ammo+" )]
	public partial class AmmoBooster : Equipment
	{
		public override WeaponConfig Config => new AmmoBoosterConfig();
		public override bool IsPassive => true;

		public override void Restock()
		{
			if ( Owner is HoverPlayer player )
			{
				foreach ( var weapon in player.Loadout.Weapons )
				{
					if ( weapon.Ammo > 0 )
					{
						player.GiveAmmo( weapon.AmmoType, ( weapon.Ammo * 0.3f ).CeilToInt() );
					}
				}
			}

			base.Restock();
		}
	}
}
