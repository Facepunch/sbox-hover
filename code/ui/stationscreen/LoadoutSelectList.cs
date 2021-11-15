using Sandbox.UI;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class LoadoutSelectList : Panel
	{
		public LoadoutSelectItem Selected { get; private set; }
		public Action<BaseLoadout> OnLoadoutSelected { get; set; }

		public void Populate( Player player )
		{
			DeleteChildren();

			var loadouts = Library.GetAll<BaseLoadout>();

			foreach ( var type in loadouts )
			{
				if ( type == typeof( BaseLoadout ) )
					continue;

				var loadout = Library.Create<BaseLoadout>( type );

				if ( loadout.UpgradesTo == null || !player.HasLoadoutUpgrade( loadout.UpgradesTo ) )
				{
					if ( loadout.UpgradeCost == 0 || player.HasLoadoutUpgrade( type ) )
					{
						var child = AddChild<LoadoutSelectItem>();
						child.SetLoadout( loadout );
					}
				}
			}

			SortChildren<LoadoutSelectItem>( ( panel ) =>
			{
				return panel.Loadout.DisplayOrder;
			} );

			foreach ( var item in ChildrenOfType<LoadoutSelectItem>() )
			{
				if ( player.Loadout.GetType() == item.Loadout.GetType() )
				{
					SetSelectedItem( item );
					break;
				}
			}
		}

		public void SetSelectedItem( LoadoutSelectItem item )
		{
			if ( Selected != null )
			{
				Selected.SetClass( "is-selected", false );
			}

			item.SetClass( "is-selected", true );

			Selected = item;

			OnLoadoutSelected?.Invoke( item.Loadout );
		}
	}
}
