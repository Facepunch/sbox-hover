﻿using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class MediumPillager : BaseLoadout
	{
		public override string Description => "A menacing unit with a deadly grenade launcher";
		public override string Name => "Medium Pillager";
		public override Type UpgradesTo => typeof( MediumPillagerMk2 );
		public override int DisplayOrder => 6;
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new BarageConfig(),
			},
			new WeaponConfig[]
			{
				new BlasterConfig(),
				new ShotblastConfig()
			},
			new WeaponConfig[]
			{
				new RadarJammerConfig(),
				new ShieldBoosterConfig()
			}
		};
		public override float JetpackScale => 0.8f;
		public override float EnergyRegen => 10f;
		public override float RegenDelay => 20f;
		public override float Health => 1300f;
		public override float Energy => 80f;
		public override float MoveSpeed => 500f;
		public override float MaxSpeed => 1300f;

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
