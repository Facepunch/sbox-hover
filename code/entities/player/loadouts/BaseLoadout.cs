using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class BaseLoadout : BaseNetworkable
	{
		public virtual List<WeaponConfig> PrimaryWeapons => new();
		public virtual List<WeaponConfig> SecondaryWeapons => new();
		public virtual int DisplayOrder => 0;
		public virtual float DownSlopeBoost => 100f;
		public virtual float UpSlopeFriction => 0.3f;
		public virtual float HealthRegen => 50f;
		public virtual float RegenDelay => 5f;
		public virtual Type UpgradesTo => null;
		public virtual float Health => 500f;
		public virtual float Energy => 100f;
		public virtual float MoveSpeed => 400f;
		public virtual float MaxSpeed => 1000f;
		public virtual string Model => "models/citizen/citizen.vmdl";
		public virtual string SecondaryDescription => "";
		public virtual string Description => "";
		public virtual string Name => "Loadout";
		public virtual int UpgradeCost => 0;
		public virtual int TokenCost => 1000;
		public virtual List<string> Clothing => new();

		public WeaponConfig PrimaryWeapon { get; set; }
		public WeaponConfig SecondaryWeapon { get; set; }

		public virtual void SetWeapons( string primaryWeapon, string secondaryWeapon )
		{
			if ( !string.IsNullOrEmpty( primaryWeapon ) )
			{
				var match = PrimaryWeapons.Find( v => v.Name == primaryWeapon );

				if ( match != null )
					PrimaryWeapon = match;
			}

			if ( !string.IsNullOrEmpty( secondaryWeapon ) )
			{
				var match = SecondaryWeapons.Find( v => v.Name == secondaryWeapon );

				if ( match != null )
					SecondaryWeapon = match;
			}
		}

		public virtual void Restock( Player player )
		{
			player.ClearAmmo();

			if ( PrimaryWeapon != null )
				player.GiveAmmo( PrimaryWeapon.AmmoType, PrimaryWeapon.Ammo );

			if ( SecondaryWeapon != null )
				player.GiveAmmo( SecondaryWeapon.AmmoType, SecondaryWeapon.Ammo );

			player.RestockWeaponUpgrades();
		}

		public virtual void SupplyLoadout( Player player )
		{
			player.Inventory.DeleteContents();

			PrimaryWeapon ??= PrimaryWeapons.FirstOrDefault();
			SecondaryWeapon ??= SecondaryWeapons.FirstOrDefault();

			if ( SecondaryWeapon != null )
			{
				var weapon = Library.Create<Weapon>( SecondaryWeapon.ClassName );
				player.Inventory.Add( weapon );
				player.ActiveChild = weapon;
			}

			if ( PrimaryWeapon != null )
			{
				var weapon = Library.Create<Weapon>( PrimaryWeapon.ClassName );
				player.Inventory.Add( weapon );
				player.ActiveChild = weapon;
			}

			Restock( player );
		}

		public virtual void Setup( Player player )
		{
			player.ClearAmmo();
			player.Inventory.DeleteContents();

			player.RemoveClothing();
			player.SetModel( Model );

			foreach ( var model in Clothing )
			{
				var clothes = player.AttachClothing( model );
				clothes.RenderColor = player.Team.GetColor();
			}

			player.Controller = new MoveController
			{
				UpSlopeFriction = UpSlopeFriction,
				DownSlopeBoost = DownSlopeBoost,
				MoveSpeed = MoveSpeed,
				MaxSpeed = MaxSpeed,
				MaxEnergy = Energy,
				Energy = Energy
			};

			player.Camera = new FirstPersonCamera();

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
