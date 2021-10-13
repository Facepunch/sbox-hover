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
		public override int TokenCost => 1000;
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

		public override void Restock( Player player )
		{
			base.Restock( player );

			player.GiveAmmo( AmmoType.Rifle, 20 );
			player.GiveAmmo( AmmoType.Shotgun, 30 );
			player.RestockWeaponUpgrades();
		}

		public override void SupplyLoadout( Player player )
		{
			base.SupplyLoadout( player );

			var pulsar = new Pulsar();
			player.Inventory.Add( pulsar );

			var shotblast = new Shotblast();
			player.Inventory.Add( shotblast, true );
			player.ActiveChild = shotblast;

			Restock( player );
		}

		public override void Setup( Player player )
		{
			base.Setup( player );

			player.AttachClothing<Jetpack>();
		}
	}
}
