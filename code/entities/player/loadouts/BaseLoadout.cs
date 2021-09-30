using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[DisallowMultipleComponent]
	public partial class BaseLoadout : EntityComponent<Player>
	{
		public virtual float Health => 500f;
		public virtual float Energy => 100f;
		public virtual float MoveSpeed => 400f;
		public virtual float MaxSpeed => 1000f;

		public virtual void SupplyLoadout()
		{

		}

		public virtual void Setup()
		{
			Entity.MoveSpeed = MoveSpeed;
			Entity.MaxSpeed = MaxSpeed;
			Entity.MaxEnergy = Energy;
			Entity.Energy = Energy;
			Entity.MaxHealth = Health;
			Entity.Health = Health;
		}
	}
}
