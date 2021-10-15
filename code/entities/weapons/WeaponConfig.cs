using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public abstract class WeaponConfig
	{
		public virtual string Name => "";
		public virtual string ClassName => "";
		public virtual string Description => "";
		public virtual string SecondaryDescription => "";
		public virtual string Icon => "";
		public virtual AmmoType AmmoType => AmmoType.Pistol;
		public virtual int Ammo => 0;
	}
}
