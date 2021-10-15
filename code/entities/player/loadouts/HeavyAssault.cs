using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyAssault : BaseLoadout
	{
		public override string Description => "A slow assault unit with high health and medium energy";
		public override string Name => "Heavy Assault";
		public override int TokenCost => 1000;
		public override LoadoutArmorType ArmorType => LoadoutArmorType.Heavy;
		public override int DisplayOrder => 5;
		public override Type UpgradesTo => typeof( HeavyAssaultMk2 );
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new PulsarConfig(),
			},
			new WeaponConfig[]
			{
				new ShotblastConfig(),
				new BlasterConfig()
			},
			new WeaponConfig[]
			{
				new ShieldBoosterConfig(),
				new AmmoBoosterConfig(),
				new SpeedBoosterConfig()
			}
		};
		public override float JetpackScale => 0.7f;
		public override float EnergyRegen => 15f;
		public override float Health => 1100f;
		public override float Energy => 75f;
		public override float RegenDelay => 20f;
		public override float MoveSpeed => 350f;
		public override float MaxSpeed => 1000f;

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
