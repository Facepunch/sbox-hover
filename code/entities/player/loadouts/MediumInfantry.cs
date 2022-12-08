using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class MediumInfantry : BaseLoadout
	{
		public override string Description => "A balanced soldier unit with medium-high health and lower energy";
		public override string Name => "Infantry";
		public override LoadoutArmorType ArmorType => LoadoutArmorType.Medium;
		public override LoadoutRoleType RoleType => LoadoutRoleType.Attacker;
		public override List<LoadoutTag> Tags => new()
		{
			new LoadoutTag( LoadoutTagType.Primary, "Flag Chaser" ),
			new LoadoutTag( LoadoutTagType.Secondary, "Attacker" ),
			new LoadoutTag( LoadoutTagType.Tertiary, "Balanced" )
		};
		public override Type UpgradesTo => typeof( MediumInfantryMk2 );
		public override int DisplayOrder => 4;
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new CarbineConfig()
			},
			new WeaponConfig[]
			{
				new BoomerConfig(),
				new RazorConfig()
			},
			new WeaponConfig[]
			{
				new EnergyBoosterConfig(),
				new AmmoBoosterConfig()
			}
		};
		public override float JetpackScale => 0.85f;
		public override float EnergyRegen => 10f;
		public override float RegenDelay => 20f;
		public override float Health => 1200f;
		public override float Energy => 80f;
		public override float MoveSpeed => 550f;
		public override float MaxSpeed => 1300f;

		public override List<string> Clothing => new()
		{
			"black_boots",
			"tactical_helmet_army",
			"longsleeve",
			"chest_armour",
			"trousers.smart"
		};

		public override void Respawn( Player player )
		{
			base.Respawn( player );

			player.AttachClothing<Jetpack>();
		}
	}
}
