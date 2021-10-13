using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class BaseLoadout : BaseNetworkable
	{
		public virtual List<string> WeaponIcons => new();
		public virtual string DisplayWeapon => "models/weapons/w_blaster.vmdl";
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

		public virtual void Restock( Player player )
		{
			player.ClearAmmo();
		}

		public virtual void SupplyLoadout( Player player )
		{
			player.ClearAmmo();
			player.Inventory.DeleteContents();
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
