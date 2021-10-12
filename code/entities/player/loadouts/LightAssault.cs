using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightAssault : BaseLoadout
	{
		public override string Description => "A fast assault unit with medium health and high energy.";
		public override string Name => "Light Assault";
		public override int DisplayOrder => 1;
		public override int TokenCost => 300;
		public override List<string> WeaponIcons => new()
		{
			"ui/weapons/pulsar.png",
			"ui/weapons/blaster.png"
		};
		public override float RegenDelay => 20f;
		public override float Health => 500f;
		public override float Energy => 100f;
		public override float MoveSpeed => 400f;
		public override float MaxSpeed => 1500f;

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
			Entity.GiveAmmo( AmmoType.SMG, 90 );
		}

		public override void SupplyLoadout()
		{
			base.SupplyLoadout();

			var pulsar = new Pulsar();
			Entity.Inventory.Add( pulsar );

			var blaster = new Blaster();
			Entity.Inventory.Add( blaster, true );
			Entity.ActiveChild = blaster;

			Restock();
		}

		public override void Setup()
		{
			base.Setup();

			Entity.AttachClothing<Jetpack>();
		}
	}
}
