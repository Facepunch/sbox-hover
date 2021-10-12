using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyTank : BaseLoadout
	{
		public override string Description => "A slow tank unit with high health, low energy and fast regen.";
		public override string Name => "Heavy Tank";
		public override int DisplayOrder => 4;
		public override List<string> WeaponIcons => new()
		{
			"ui/weapons/shotblast.png",
			"ui/weapons/barage.png"
		};
		public override float HealthRegen => 100f;
		public override float RegenDelay => 5f;
		public override float Health => 1000f;
		public override float Energy => 30f;
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

		public override void Restock()
		{
			base.Restock();

			Entity.GiveAmmo( AmmoType.Grenade, 20 );
			Entity.GiveAmmo( AmmoType.Shotgun, 30 );
		}

		public override void SupplyLoadout()
		{
			base.SupplyLoadout();

			var barage = new Barage();
			Entity.Inventory.Add( barage );

			var shotblast = new Shotblast();
			Entity.Inventory.Add( shotblast, true );
			Entity.ActiveChild = shotblast;

			Restock();
		}

		public override void Setup()
		{
			base.Setup();

			Entity.AttachClothing<Jetpack>();
		}
	}
}
