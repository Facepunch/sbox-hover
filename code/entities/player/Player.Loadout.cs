using Sandbox;

namespace Facepunch.Hover
{
	public partial class Player
	{
		public BaseLoadout Loadout
		{
			get => Components.Get<BaseLoadout>();
		}

		public BaseLoadout GiveLoadout( BaseLoadout loadout )
		{
			foreach ( var component in Components.GetAll<BaseLoadout>() )
			{
				component.Remove();
			}

			Components.Add<BaseLoadout>( loadout );

			return loadout;
		}

		public BaseLoadout GiveLoadout<T>() where T : BaseLoadout, new()
		{
			foreach ( var loadout in Components.GetAll<T>() )
			{
				loadout.Remove();
			}

			return Components.Create<T>();
		}
	}
}
