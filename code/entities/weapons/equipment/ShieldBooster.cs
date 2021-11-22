using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class ShieldBoosterConfig : WeaponConfig
	{
		public override string Name => "Shield+";
		public override string Description => "-20% Energy Regen";
		public override string SecondaryDescription => "Energy Absorbs Damage";
		public override string Icon => "ui/equipment/shield_booster.png";
		public override string ClassName => "hv_shield_booster";
	}

	[Library( "hv_shield_booster", Title = "Shield+" )]
	public partial class ShieldBooster : Equipment
	{
		public override WeaponConfig Config => new ShieldBoosterConfig();
		public override bool IsPassive => true;

		public override DamageInfo OwnerTakeDamage( DamageInfo info )
		{
			if ( Owner is Player player )
			{
				info.Damage = Math.Max( info.Damage - player.Energy, 0f );
				player.Energy *= 0.5f;
			}

			return info;
		}

		protected override void OnEquipmentGiven( Player player )
		{
			player.EnergyRegen *= 0.8f;
		}

		protected override void OnEquipmentTaken( Player player )
		{
			player.EnergyRegen *= 1f / 0.8f;
		}
	}
}
