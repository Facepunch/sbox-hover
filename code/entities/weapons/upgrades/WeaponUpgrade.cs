﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public abstract partial class WeaponUpgrade
	{
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual int TokenCost => 50;

		public virtual void Restock( Player player, Weapon weapon )
		{

		}

		public virtual void Apply( Player player, Weapon weapon )
		{
			
		}
	}
}