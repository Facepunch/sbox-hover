using Sandbox;

namespace Facepunch.Hover
{
	public partial class HoverPlayer
	{
		[Net] public BaseLoadout Loadout { get; set; }

		public BaseLoadout GiveLoadout( BaseLoadout loadout )
		{
			if ( Loadout.GetType() != loadout.GetType() )
			{
				Loadout = loadout;
				Loadout.Initialize( this );
			}

			return Loadout;
		}

		public BaseLoadout GiveLoadout<T>() where T : BaseLoadout, new()
		{
			Loadout = new T();
			return Loadout;
		}
	}
}
