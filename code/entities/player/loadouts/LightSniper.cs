using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightSniper : BaseLoadout
	{
		public override string Description => "A fast sniper unit with low health and medium energy.";
		public override string Name => "Light Sniper";
		public override int DisplayOrder => 2;
		public override List<string> WeaponIcons => new()
		{
			"ui/weapons/longshot.png",
			"ui/weapons/sideman.png"
		};
		public override float RegenDelay => 20f;
		public override float Health => 200f;
		public override float Energy => 80f;
		public override float MoveSpeed => 350f;
		public override float MaxSpeed => 1200f;

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

			Entity.GiveAmmo( AmmoType.Rifle, 20 );
			Entity.GiveAmmo( AmmoType.Pistol, 60 );
		}

		public override void SupplyLoadout()
		{
			base.SupplyLoadout();

			var sideman = new Sideman();
			Entity.Inventory.Add( sideman );

			var longshot = new Longshot();
			Entity.Inventory.Add( longshot, true );
			Entity.ActiveChild = longshot;

			Restock();
		}

		public override void Setup()
		{
			base.Setup();

			Entity.AttachClothing<Jetpack>();
		}
	}
}
