using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public struct WeaponStat
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}

	public enum WeaponType
	{
		None,
		Hitscan,
		Projectile,
		Equipment,
		Deployable
	}

	public abstract class WeaponConfig
	{
		public virtual string Name => "";
		public virtual string ClassName => "";
		public virtual string Description => "";
		public virtual string SecondaryDescription => "";
		public virtual string Icon => "";
		public virtual AmmoType AmmoType => AmmoType.Pistol;
		public virtual List<Type> Upgrades => null;
		public virtual WeaponType Type => WeaponType.None;
		public virtual int Damage => 0;
		public virtual int Ammo => 0;

		public virtual List<WeaponStat> GetStats()
		{
			var stats = new List<WeaponStat>();

			if ( Type != WeaponType.None )
			{
				stats.Add( new WeaponStat
				{
					Key = "Type",
					Value = Type.ToString()
				} );
			}

			if ( Damage > 0 )
			{
				stats.Add( new WeaponStat
				{
					Key = "Damage",
					Value = Damage.ToString()
				} );
			}

			if ( Ammo > 0 )
			{
				stats.Add( new WeaponStat
				{
					Key = "Ammo",
					Value = $"x{Ammo}"
				} );
			}

			return stats;
		}
	}
}
