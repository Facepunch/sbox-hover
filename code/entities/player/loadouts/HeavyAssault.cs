using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyAssault : BaseLoadout
	{
		public override string Description => "A slow assault unit with high health and medium energy.";
		public override string Name => "Heavy Assault";
		public override int DisplayOrder => 3;
		public override List<string> WeaponIcons => new()
		{
			"ui/weapons/shotblast.png",
			"ui/weapons/pulsar.png"
		};
		public override float RegenDelay => 20f;
		public override float Health => 700f;
		public override float Energy => 60f;
		public override float MoveSpeed => 300f;
		public override float MaxSpeed => 1000f;

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

			var pulsar = new Pulsar();
			Entity.Inventory.Add( pulsar );

			var shotblast = new Shotblast();
			Entity.Inventory.Add( shotblast, true );
			Entity.ActiveChild = shotblast;

			Entity.GiveAmmo( AmmoType.Rifle, 20 );
			Entity.GiveAmmo( AmmoType.Shotgun, 30 );
		}

		public override void Setup()
		{
			base.Setup();

			Entity.AttachClothing<Jetpack>();
		}
	}
}
