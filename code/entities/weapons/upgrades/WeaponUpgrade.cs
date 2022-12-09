using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public abstract partial class WeaponUpgrade
	{
		public virtual string Icon => "ui/icons/icon_upgrade.png";
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual int TokenCost => 50;

		public virtual DamageInfo DealDamage( HoverPlayer player, HoverPlayer victim, Weapon weapon, DamageInfo info )
		{
			return info;
		}

		public virtual void Restock( HoverPlayer player, Weapon weapon )
		{

		}

		public virtual void Apply( HoverPlayer player, Weapon weapon )
		{
			
		}
	}
}
