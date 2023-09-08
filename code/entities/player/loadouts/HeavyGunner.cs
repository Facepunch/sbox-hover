using Sandbox;
using System.Collections.Generic;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyGunner : BaseLoadout
	{
		public override string Description => "A slow heavy support unit with high health and medium energy";
		public override string Name => "Gunner";
		public override LoadoutArmorType ArmorType => LoadoutArmorType.Heavy;
		public override LoadoutRoleType RoleType => LoadoutRoleType.Support;
		public override float Availability => 0.3f;
		public override int DisplayOrder => 8;
		public override List<LoadoutTag> Tags => new()
		{
			new LoadoutTag( LoadoutTagType.Primary, "Support" ),
			new LoadoutTag( LoadoutTagType.Secondary, "Suppression" ),
			new LoadoutTag( LoadoutTagType.Tertiary, "Traps" )
		};
		public override Type UpgradesTo => typeof( HeavyGunnerMk2 );
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new FusionConfig(),
			},
			new WeaponConfig[]
			{
				new ClusterConfig(),
				new ShotblastConfig()
			},
			new WeaponConfig[]
			{
				new DeployableForceShieldConfig()
			},
			new WeaponConfig[]
			{
				new DeployableJumpMineConfig()
			}
		};
		public override float JetpackScale => 0.75f;
		public override float EnergyRegen => 15f;
		public override float Health => 2000f;
		public override float Energy => 75f;
		public override float RegenDelay => 20f;
		public override float MoveSpeed => 425f;
		public override float MaxSpeed => 1600f;

		public override List<string> Clothing => new()
		{
			"light_shoes",
			"light_helmet",
			"light_chest",
			"light_gloves",
			"light_legs"
		};

		public override void Respawn( HoverPlayer player )
		{
			base.Respawn( player );

			player.AttachClothing<Jetpack>();
		}
	}
}
