using Sandbox;
using System.Collections.Generic;
using System;

namespace Facepunch.Hover
{
	[Library]
	public partial class HeavyAssault : BaseLoadout
	{
		public override string Description => "A slow assault unit with high health and medium energy";
		public override string Name => "Assault";
		public override LoadoutArmorType ArmorType => LoadoutArmorType.Heavy;
		public override LoadoutRoleType RoleType => LoadoutRoleType.Attacker;
		public override float Availability => 0.3f;
		public override int DisplayOrder => 7;
		public override List<LoadoutTag> Tags => new()
		{
			new LoadoutTag( LoadoutTagType.Primary, "Attacker" ),
			new LoadoutTag( LoadoutTagType.Secondary, "Splash Damage" ),
			new LoadoutTag( LoadoutTagType.Tertiary, "Hard-Hitter" )
		};
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
