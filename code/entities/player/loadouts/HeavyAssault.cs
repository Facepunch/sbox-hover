﻿using Gamelib.Utility;
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
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new PulsarConfig(),
			},
			new WeaponConfig[]
			{
				new ShotblastConfig(),
				new BlasterConfig()
			},
			new WeaponConfig[]
			{
				new ShieldBoosterConfig(),
				new AmmoBoosterConfig(),
				new SpeedBoosterConfig()
			}
		};
		public override float JetpackScale => 0.7f;
		public override float EnergyRegen => 10f;
		public override float RegenDelay => 20f;
		public override float Health => 1000f;
		public override float Energy => 75f;
		public override float MoveSpeed => 350f;
		public override float MaxSpeed => 1000f;

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
