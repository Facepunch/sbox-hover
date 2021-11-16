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
		public override LoadoutArmorType ArmorType => LoadoutArmorType.Heavy;
		public override int DisplayOrder => 7;
		public override Type UpgradesTo => typeof( HeavyAssaultMk2 );
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new BigPulsarConfig(),
			},
			new WeaponConfig[]
			{
				new ShotblastConfig(),
				new RazorConfig()
			},
			new WeaponConfig[]
			{
				new ShieldBoosterConfig(),
				new AmmoBoosterConfig(),
				new SpeedBoosterConfig()
			}
		};
		public override float JetpackScale => 0.75f;
		public override float EnergyRegen => 15f;
		public override float Health => 2000f;
		public override float Energy => 75f;
		public override float RegenDelay => 20f;
		public override float MoveSpeed => 425f;
		public override float MaxSpeed => 1100f;

		public override List<string> Clothing => new()
		{
			CitizenClothing.Shoes.WorkBoots,
			CitizenClothing.Trousers.Police,
			CitizenClothing.Shirt.Longsleeve.Plain,
			CitizenClothing.Jacket.Heavy,
			CitizenClothing.Hat.SecurityHelmet.Normal
		};

		public override void Respawn( Player player )
		{
			base.Respawn( player );

			player.AttachClothing<Jetpack>();
		}
	}
}
