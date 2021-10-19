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
		public override string Name => "Light Saboteur";
		public override int DisplayOrder => 2;
		public override Type UpgradesTo => typeof( LightSaboteurMk2 );
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new BurstConfig(),
				new PulsarConfig()
			},
			new WeaponConfig[]
			{
				new StickyConfig()
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
		public override float MoveSpeed => 350f;
		public override float MaxSpeed => 1500f;

		public override List<string> Clothing => new()
		{
			CitizenClothing.Shoes.WorkBoots,
			CitizenClothing.Trousers.Police,
			CitizenClothing.Shirt.Longsleeve.Plain,
			CitizenClothing.Jacket.Heavy,
			CitizenClothing.Hat.SecurityHelmet.Normal
		};

		public override void Setup( Player player )
		{
			base.Setup( player );

			player.AttachClothing<Jetpack>();
		}
	}
}
