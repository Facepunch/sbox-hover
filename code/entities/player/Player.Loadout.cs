using Sandbox;

namespace Facepunch.Hover
{
	public partial class Player
	{
		public BaseLoadout Loadout
		{
			get => Components.Get<BaseLoadout>();
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
