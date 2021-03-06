using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightSaboteur : BaseLoadout
	{
		public override string Description => "A fast stealth unit with lower health and medium energy";
		public override string Name => "Saboteur";
		public override LoadoutRoleType RoleType => LoadoutRoleType.Support;
		public override List<LoadoutTag> Tags => new()
		{
			new LoadoutTag( LoadoutTagType.Primary, "Flag Chaser" ),
			new LoadoutTag( LoadoutTagType.Secondary, "Support" ),
			new LoadoutTag( LoadoutTagType.Tertiary, "Fast" ),
			new LoadoutTag( LoadoutTagType.Quaternary, "Stealth" )
		};
		public override int DisplayOrder => 2;
		public override Type UpgradesTo => typeof( LightSaboteurMk2 );
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new BurstConfig()
			},
			new WeaponConfig[]
			{
				new StickyConfig(),
				new RazorConfig()
			},
			new WeaponConfig[]
			{
				new StealthCamoConfig()
			}
		};
		public override float RegenDelay => 20f;
		public override float Health => 700f;
		public override float Energy => 90f;
		public override float EnergyRegen => 15f;
		public override float MoveSpeed => 550f;
		public override float MaxSpeed => 1500f;

		public override List<string> Clothing => new()
		{
			CitizenClothing.Shoes.WorkBoots,
			CitizenClothing.Trousers.Police,
			CitizenClothing.Shirt.Longsleeve.Plain,
			CitizenClothing.Jacket.Heavy,
			CitizenClothing.Hat.SecurityHelmet.Normal
		};

		public override void Respawn( Player player )
		{
			base.Respawn( player );

			player.AttachClothing<Jetpack>();
		}
	}
}
