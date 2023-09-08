﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public enum LoadoutArmorType
	{
		Light,
		Medium,
		Heavy
	}

	public enum LoadoutRoleType
	{
		Attacker,
		Defender,
		Support
	}

	public enum LoadoutTagType
	{
		Primary,
		Secondary,
		Tertiary,
		Quaternary
	}

	public struct LoadoutTag
	{
		public LoadoutTagType Type { get; set; }
		public string Name { get; set; }

		public LoadoutTag( LoadoutTagType type, string name )
		{
			Type = type;
			Name = name;
		}
	}

	public partial class BaseLoadout : BaseNetworkable
	{
		public virtual WeaponConfig[][] AvailableWeapons => new WeaponConfig[][] { };
		public virtual LoadoutArmorType ArmorType => LoadoutArmorType.Light;
		public virtual LoadoutRoleType RoleType => LoadoutRoleType.Attacker;
		public virtual List<LoadoutTag> Tags => new();
		public virtual int DisplayOrder => 0;
		public virtual bool CanUpgradeDependencies => false;
		public virtual bool CanRepairGenerator => false;
		public virtual float Availability => 1f;
		public virtual int Level => 1;
		public virtual float HealthRegen => 50f;
		public virtual float EnergyRegen => 20f;
		public virtual float EnergyDrain => 20f;
		public virtual float RegenDelay => 5f;
		public virtual Type UpgradesTo => null;
		public virtual float JetpackScale => 1f;
		public virtual float Health => 500f;
		public virtual float Energy => 100f;
		public virtual float MoveSpeed => 400f;
		public virtual float MaxSpeed => 2400f;
		public virtual string Model => "models/citizen/citizen.vmdl";
		public virtual string SecondaryDescription => "";
		public virtual string Description => "";
		public virtual string Name => "Loadout";
		public virtual int UpgradeCost => 0;
		public virtual List<string> Clothing => new();

		public WeaponConfig[] Weapons { get; set; }

		public BaseLoadout()
		{
			Weapons = new WeaponConfig[AvailableWeapons.Length];

			for ( int i = Weapons.Length - 1; i >= 0; i-- )
			{
				var weapon = Weapons[i];

				if ( weapon == null )
				{
					weapon = AvailableWeapons[i].FirstOrDefault();
					Weapons[i] = weapon;
				}
			}
		}

		public int GetTotalPlayers()
		{
			return Entity.All
				.OfType<HoverPlayer>()
				.Where( p =>
					p.Loadout is not null )
				.Count( p => p.Loadout.IsTheSameAs( this ) );
		}

		public int GetTotalAllowed()
		{
			var players = Entity.All.OfType<HoverPlayer>();
			return (players.Count() * Availability).CeilToInt();
		}

		public bool IsTheSameAs( BaseLoadout loadout )
		{
			return loadout.GetType().IsAssignableFrom( GetType() ) || GetType().IsAssignableTo( loadout.GetType() );
		}

		public virtual bool IsAvailable()
		{
			return GetTotalPlayers() < GetTotalAllowed();
		}

		public virtual string GetSlotName( int slot )
		{
			return slot switch
			{
				0 => "Primary",
				1 => "Secondary",
				_ => "Equipment",
			};
		}

		public virtual void UpdateWeapons( params string[] weapons )
		{
			for ( int i = 0; i < weapons.Length; i++ )
			{
				var weapon = weapons[i];

				if ( AvailableWeapons.Length > i )
				{
					foreach ( var match in AvailableWeapons[i] )
					{
						if ( match.Name == weapon )
						{
							Weapons[i] = match;
							break;
						}
					}
				}
			}
		}

		public virtual void Restock( HoverPlayer player )
		{
			player.ClearAmmo();

			foreach ( var weapon in Weapons )
			{
				if ( weapon != null && weapon.Ammo > 0 )
				{
					player.GiveAmmo( weapon.AmmoType, weapon.Ammo );
				}
			}

			foreach ( var weapon in player.Children.OfType<Weapon>() )
			{
				weapon.Restock();
			}

			player.RestockWeaponUpgrades();
		}

		public virtual void Supply( HoverPlayer player )
		{
			player.Inventory.DeleteContents();

			for ( int i = Weapons.Length - 1; i >= 0; i-- )
			{
				var weapon = Weapons[i];

				if ( weapon != null )
				{
					var entity = TypeLibrary.Create<Weapon>( weapon.ClassName );
					player.Inventory.Add( entity );
					player.ActiveChild = entity;
					entity.Slot = i + 1;
				}
			}

			player.ApplyWeaponUpgrades();

			Restock( player );
		}

		public virtual void Initialize( HoverPlayer player )
		{
			var deployables = Entity.All.OfType<DeployableEntity>()
				.Where( v => v.Deployer == player );

			foreach ( var deployable in deployables )
			{
				deployable.Explode();
			}
		}

		public virtual void Respawn( HoverPlayer player )
		{
			player.ClearAmmo();
			player.Inventory.DeleteContents();

			player.RemoveClothing();
			player.SetModel( Model );

			var allClothing = ResourceLibrary.GetAll<Clothing>();

			foreach ( var assetName in Clothing )
			{
				var modelName = allClothing
					.Where( c => c.ResourceName.ToLower() == assetName.ToLower() )
					.Select( c => c.Model )
					.FirstOrDefault();

				var clothes = player.AttachClothing( modelName );
				clothes.RenderColor = player.Team.GetColor();
				clothes.SetMaterialGroup( player.Team == Team.Red ? 1 : 2 );
				//clothes.SetMaterialOverride( Material.Load( player.Team.GetTeamSkin() ), "skin" );
			}

			player.Controller = new MoveController
			{
				JetpackScale = JetpackScale,
				MoveSpeed = MoveSpeed,
				MaxSpeed = MaxSpeed
			};

			player.EnergyRegen = EnergyRegen;
			player.EnergyDrain = EnergyDrain;
			player.MaxEnergy = Energy;
			player.Energy = Energy;

			player.HealthRegen = HealthRegen;
			player.RegenDelay = RegenDelay;
			player.MaxHealth = Health;
			player.Health = Health;

			player.RemoveAllDecals();

			player.EnableAllCollisions = true;
			player.EnableDrawing = true;
			player.EnableHideInFirstPerson = true;
			player.EnableShadowInFirstPerson = true;
		}
	}
}
