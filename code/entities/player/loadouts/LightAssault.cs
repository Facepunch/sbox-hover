using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class LightAssault : BaseLoadout
	{
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

			var longshot = new Longshot();
			Entity.Inventory.Add( longshot );

			var blaster = new Blaster();
			Entity.Inventory.Add( blaster, true );
			Entity.ActiveChild = blaster;

			Entity.GiveAmmo( AmmoType.Pistol, 120 );
		}

		public override void Setup()
		{
			base.Setup();

			Entity.AttachClothing<Jetpack>();
		}
	}
}
