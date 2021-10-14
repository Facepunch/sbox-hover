using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyTank : BaseLoadout
	{
		public override string Description => "A slow tank unit with high health, low energy and fast regen.";
		public override string Name => "Heavy Tank";
		public override LoadoutArmorType ArmorType => LoadoutArmorType.Heavy;
		public override int DisplayOrder => 4;
		public override Type UpgradesTo => typeof( HeavyTankMk2 );
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new ShotblastConfig(),
			},
			new WeaponConfig[]
			{
				new BarageConfig()
			},
			new WeaponConfig[]
			{
				new HeavyShieldBoosterConfig(),
				new HeavyEnergyBoosterConfig()
			}
		};
		public override float JetpackScale => 0.7f;
		public override float HealthRegen => 100f;
		public override float RegenDelay => 15f;
		public override float EnergyRegen => 10f;
		public override int TokenCost => 1500;
		public override float Health => 1500f;
		public override float Energy => 60f;
		public override float MoveSpeed => 300f;
		public override float MaxSpeed => 800f;

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
