using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[DisallowMultipleComponent]
	public partial class BaseLoadout : EntityComponent<Player>
	{
		public virtual float HealthRegen => 50f;
		public virtual float RegenDelay => 5f;
		public virtual float Health => 500f;
		public virtual float Energy => 100f;
		public virtual float MoveSpeed => 400f;
		public virtual float MaxSpeed => 1000f;
		public virtual string Model => "models/citizen/citizen.vmdl";
		public virtual List<string> Clothing => new();

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
				Entity.AttachClothing( model );
			}

			Entity.HealthRegen = HealthRegen;
			Entity.RegenDelay = RegenDelay;
			Entity.MoveSpeed = MoveSpeed;
			Entity.MaxSpeed = MaxSpeed;
			Entity.MaxEnergy = Energy;
			Entity.Energy = Energy;
			Entity.MaxHealth = Health;
			Entity.Health = Health;

			Entity.RemoveAllDecals();

			Entity.EnableAllCollisions = true;
			Entity.EnableDrawing = true;
			Entity.EnableHideInFirstPerson = true;
			Entity.EnableShadowInFirstPerson = true;

			Entity.Controller = new MoveController();
			Entity.Camera = new FirstPersonCamera();
		}
	}
}
