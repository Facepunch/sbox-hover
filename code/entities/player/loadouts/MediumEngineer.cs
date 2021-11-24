using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class MediumEngineer : BaseLoadout
	{
		public override string Description => "A unit that can deploy light turrets and upgrade base defences";
		public override string Name => "Engineer";
		public override LoadoutArmorType ArmorType => LoadoutArmorType.Medium;
		public override LoadoutRoleType RoleType => LoadoutRoleType.Defender;
		public override List<LoadoutTag> Tags => new()
		{
			new LoadoutTag( LoadoutTagType.Primary, "Engineer" ),
			new LoadoutTag( LoadoutTagType.Secondary, "Defender" ),
			new LoadoutTag( LoadoutTagType.Tertiary, "Upgrading" ),
			new LoadoutTag( LoadoutTagType.Tertiary, "Repairing" )
		};
		public override Type UpgradesTo => typeof( MediumEngineerMk2 );
		public override bool CanUpgradeDependencies => true;
		public override bool CanRepairGenerator => true;
		public override int DisplayOrder => 5;
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new BlasterConfig(),
				new BoomerConfig()
			},
			new WeaponConfig[]
			{
				new RazorConfig()
			},
			new WeaponConfig[]
			{
				new DeployableTurretConfig()
			},
			new WeaponConfig[]
			{
				new DeployableMotionAlarmConfig()
			}
		};
		public override float JetpackScale => 0.85f;
		public override float EnergyRegen => 10f;
		public override float RegenDelay => 20f;
		public override float Health => 1200f;
		public override float Energy => 80f;
		public override float MoveSpeed => 500f;
		public override float MaxSpeed => 1000f;

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
