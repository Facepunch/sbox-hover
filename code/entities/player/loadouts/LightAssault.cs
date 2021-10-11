using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class LightAssault : BaseLoadout
	{
		public override float MaxSpeed => 1500f;

		public override List<string> Clothing => new()
		{
			CitizenClothing.Shoes.WorkBoots,
			CitizenClothing.Trousers.Police,
			CitizenClothing.Shirt.Longsleeve.Plain,
			CitizenClothing.Jacket.Heavy,
			CitizenClothing.Hat.SecurityHelmet.Normal
		};

		public override void SupplyLoadout()
		{
			base.SupplyLoadout();

			var sideman = new Sideman();
			Entity.Inventory.Add( sideman );

			var barage = new Barage();
			Entity.Inventory.Add( barage );

			var shotblast = new Shotblast();
			Entity.Inventory.Add( shotblast );

			var longshot = new Longshot();
			Entity.Inventory.Add( longshot );

			var pulsar = new Pulsar();
			Entity.Inventory.Add( pulsar );

			var blaster = new Blaster();
			Entity.Inventory.Add( blaster, true );
			Entity.ActiveChild = blaster;

			Entity.GiveAmmo( AmmoType.Pistol, 120 );
			Entity.GiveAmmo( AmmoType.Rifle, 120 );
			Entity.GiveAmmo( AmmoType.SMG, 120 );
			Entity.GiveAmmo( AmmoType.Sniper, 120 );
			Entity.GiveAmmo( AmmoType.Grenade, 120 );
			Entity.GiveAmmo( AmmoType.Shotgun, 120 );
		}

		public override void Setup()
		{
			base.Setup();

			Entity.AttachClothing<Jetpack>();
		}
	}
}
