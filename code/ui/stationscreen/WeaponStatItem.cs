using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class WeaponStatItem : Panel
	{
		public string Key => Stat.Key;
		public string Value => Stat.Value;
		public int Index { get; private set; }
		public WeaponConfig Config { get; private set; }
		public WeaponStat Stat { get; private set; }

		public void SetStat( WeaponStat stat )
		{
			Stat = stat;
		}
	}
}
