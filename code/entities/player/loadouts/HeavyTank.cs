using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyTank : BaseLoadout
	{
		public override string Description => "A slow tank unit with high health, low energy and fast regen";
		public override string Name => "Heavy Tank";
		public override LoadoutArmorType ArmorType => LoadoutArmorType.Heavy;
		public override int DisplayOrder => 9;
		public override Type UpgradesTo => typeof( HeavyTankMk2 );
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new DestroyerConfig(),
				new ClusterConfig()
			},
			new WeaponConfig[]
			{
				new PulsarConfig()
			},
			new WeaponConfig[]
			{
				new HeavyShieldBoosterConfig(),
				new HeavyEnergyBoosterConfig()
			}
		};
		public override float JetpackScale => 0.5f;
		public override float HealthRegen => 100f;
		public override float RegenDelay => 15f;
		public override float EnergyRegen => 10f;
		public override float Health => 1500f;
		public override float Energy => 60f;
		public override float MoveSpeed => 350f;
		public override float MaxSpeed => 900f;

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
