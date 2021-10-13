﻿using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightSniper : BaseLoadout
	{
		public override string Description => "A fast sniper unit with low health and medium energy.";
		public override string Name => "Light Sniper";
		public override int TokenCost => 800;
		public override int DisplayOrder => 2;
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new LongshotConfig(),
			},
			new WeaponConfig[]
			{
				new SidemanConfig()
			},
			new WeaponConfig[]
			{
				new EnergyBoosterConfig(),
				new SpeedBoosterConfig(),
				new AmmoBoosterConfig()
			}
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

		public override void Setup( Player player )
		{
			base.Setup( player );

			player.AttachClothing<Jetpack>();
		}
	}
}
