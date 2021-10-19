using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightAssault : BaseLoadout
	{
		public override string Description => "A fast assault unit with medium health and high energy";
		public override string Name => "Light Assault";
		public override Type UpgradesTo => typeof( LightAssaultMk2 );
		public override int DisplayOrder => 1;
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new PulsarConfig()
			},
			new WeaponConfig[]
			{
				new BlasterConfig(),
				new SidemanConfig()
			},
			new WeaponConfig[]
			{
				new EnergyBoosterConfig(),
				new AmmoBoosterConfig()
			}
		};
		public override float RegenDelay => 20f;
		public override float EnergyRegen => 15f;
		public override float Health => 800f;
		public override float Energy => 100f;
		public override float MoveSpeed => 400f;
		public override float MaxSpeed => 1800f;

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
