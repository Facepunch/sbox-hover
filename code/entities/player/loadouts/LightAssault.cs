using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public partial class LightAssault : BaseLoadout
	{
		public override string Description => "A fast assault unit with medium health and high energy";
		public override string Name => "Assault";
		public override LoadoutRoleType RoleType => LoadoutRoleType.Attacker;
		public override List<LoadoutTag> Tags => new()
		{
			new LoadoutTag( LoadoutTagType.Primary, "Flag Capper" ),
			new LoadoutTag( LoadoutTagType.Secondary, "Attacker" ),
			new LoadoutTag( LoadoutTagType.Tertiary, "Splash Damage" )
		};
		public override Type UpgradesTo => typeof( LightAssaultMk2 );
		public override int DisplayOrder => 1;
		public override WeaponConfig[][] AvailableWeapons => new WeaponConfig[][]
		{
			new WeaponConfig[]
			{
				new PulsarConfig()
			},
			new WeaponConfig[]
			{
				new BlasterConfig(),
				new SidemanConfig()
			},
			new WeaponConfig[]
			{
				new EnergyBoosterConfig(),
				new AmmoBoosterConfig()
			}
		};
		public override float RegenDelay => 20f;
		public override float EnergyRegen => 15f;
		public override float Health => 800f;
		public override float Energy => 100f;
		public override float MoveSpeed => 550f;
		public override float MaxSpeed => 1800f;

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
