using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightSniper : BaseLoadout
	{
		public override string Description => "A fast sniper unit with lower health and medium energy";
		public override string Name => "Light Sniper";
		public override int DisplayOrder => 3;
		public override Type UpgradesTo => typeof( LightSniperMk2 );
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new LongshotConfig(),
			},
			new WeaponConfig[]
			{
				new ShotblastConfig(),
				new SidemanConfig()
			},
			new WeaponConfig[]
			{
				new DeployableClaymoreConfig()
			},
			new WeaponConfig[]
			{
				new DeployableDisruptorConfig(),
				new EnergyBoosterConfig()
			}
		};
		public override float RegenDelay => 20f;
		public override float Health => 500f;
		public override float Energy => 90f;
		public override float EnergyRegen => 15f;
		public override float MoveSpeed => 550f;
		public override float MaxSpeed => 1200f;

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
