using Sandbox;

namespace Facepunch.Hover
{
	public partial class Player
	{
		[Net] public BaseLoadout Loadout { get; set; }

		public BaseLoadout GiveLoadout( BaseLoadout loadout )
		{
			Loadout = loadout;

			return loadout;
		}

		public BaseLoadout GiveLoadout<T>() where T : BaseLoadout, new()
		{
			Loadout = new T();
			return Loadout;
		}
	}
}
