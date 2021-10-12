using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[DisallowMultipleComponent]
	public partial class BaseLoadout : EntityComponent<Player>
	{
		public virtual List<string> WeaponIcons => new();
		public virtual string DisplayWeapon => "models/weapons/w_blaster.vmdl";
		public virtual int DisplayOrder => 0;
		public virtual float DownSlopeBoost => 100f;
		public virtual float UpSlopeFriction => 0.3f;
		public virtual float HealthRegen => 50f;
		public virtual float RegenDelay => 5f;
		public virtual float Health => 500f;
		public virtual float Energy => 100f;
		public virtual float MoveSpeed => 400f;
		public virtual float MaxSpeed => 1000f;
		public virtual string Model => "models/citizen/citizen.vmdl";
		public virtual string Description => "";
		public virtual string Name => "Loadout";
		public virtual int TokenCost => 1000;
		public virtual List<string> Clothing => new();

		public virtual void Restock()
		{
			Entity.ClearAmmo();
		}

		public virtual void SupplyLoadout()
		{
			Entity.ClearAmmo();
			Entity.Inventory.DeleteContents();
		}

		public virtual void Setup()
		{
			Entity.ClearAmmo();
			Entity.Inventory.DeleteContents();

			Entity.RemoveClothing();
			Entity.SetModel( Model );

			foreach ( var model in Clothing )
			{
				var clothes = Entity.AttachClothing( model );
				clothes.RenderColor = Entity.Team.GetColor();
			}

			Entity.Controller = new MoveController
			{
				UpSlopeFriction = UpSlopeFriction,
				DownSlopeBoost = DownSlopeBoost,
				MoveSpeed = MoveSpeed,
				MaxSpeed = MaxSpeed,
				MaxEnergy = Energy,
				Energy = Energy
			};

			Entity.Camera = new FirstPersonCamera();

			Entity.HealthRegen = HealthRegen;
			Entity.RegenDelay = RegenDelay;
			Entity.MaxHealth = Health;
			Entity.Health = Health;

			Entity.RemoveAllDecals();

			Entity.EnableAllCollisions = true;
			Entity.EnableDrawing = true;
			Entity.EnableHideInFirstPerson = true;
			Entity.EnableShadowInFirstPerson = true;
		}
	}
}
