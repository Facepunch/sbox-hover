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
		public override string Name => "Sniper";
		public override int DisplayOrder => 3;
		public override LoadoutRoleType RoleType => LoadoutRoleType.Defender;
		public override List<LoadoutTag> Tags => new()
		{
			new LoadoutTag( LoadoutTagType.Primary, "Long Range" ),
			new LoadoutTag( LoadoutTagType.Secondary, "Support" ),
			new LoadoutTag( LoadoutTagType.Tertiary, "Traps" )
		};
		public override Type UpgradesTo => typeof( LightSniperMk2 );
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
				new DeployableClaymoreConfig()
			},
			new WeaponConfig[]
			{
				new DeployableDisruptorConfig(),
				new EnergyBoosterConfig()
			}
		};
		public override float RegenDelay => 20f;
		public override float Health => 700f;
		public override float Energy => 90f;
		public override float EnergyRegen => 15f;
		public override float MoveSpeed => 550f;
		public override float MaxSpeed => 1200f;

		public override List<string> Clothing => new()
		{
			"black_boots",
			"tactical_helmet_army",
			"longsleeve",
			"chest_armour",
			"trousers.smart"
		};

		public override void Respawn( HoverPlayer player )
		{
			base.Respawn( player );

			player.AttachClothing<Jetpack>();
		}
	}
}
