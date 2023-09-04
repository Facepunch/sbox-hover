using Sandbox;

namespace Facepunch.Hover
{
	public partial class HoverPlayer
	{
		[Net] public BaseLoadout Loadout { get; set; }

		public BaseLoadout GiveLoadout( BaseLoadout loadout )
		{
			if ( Loadout != null && Loadout.GetType() == loadout.GetType() )
			{
				return Loadout;
			}

			Loadout = loadout;
			Loadout.Initialize( this );

			return Loadout;
		}

		public BaseLoadout GiveLoadout<T>() where T : BaseLoadout, new()
		{
			GiveLoadout( new T() );
			return Loadout;
		}
	}
}
