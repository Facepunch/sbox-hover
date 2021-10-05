using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class LightSniper : BaseLoadout
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

			Entity.Inventory.Add( new Pistol(), true );
			Entity.Inventory.Add( new Blaster(), true );
			Entity.GiveAmmo( AmmoType.Pistol, 120 );
		}

		public override void Setup()
		{
			base.Setup();

			Entity.AttachClothing<Jetpack>();
		}
	}
}
