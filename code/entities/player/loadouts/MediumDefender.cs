﻿using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class MediumDefender : BaseLoadout
	{
		public override string Description => "A unit that can deploy light turrets and upgrade base defences";
		public override string Name => "Medium Defender";
		public override Type UpgradesTo => typeof( MediumDefenderMk2 );
		public override int DisplayOrder => 5;
		public override int TokenCost => 800;
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new BlasterConfig(),
				new BarageConfig()
			},
			new WeaponConfig[]
			{
				new SidemanConfig()
			},
			new WeaponConfig[]
			{
				new DeployableTurretConfig()
			}
		};
		public override float JetpackScale => 0.8f;
		public override float EnergyRegen => 10f;
		public override float RegenDelay => 20f;
		public override float Health => 900f;
		public override float Energy => 80f;
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