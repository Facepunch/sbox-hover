using Sandbox.UI;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class LoadoutSelectList : Panel
	{
		public Panel LightLoadouts { get; private set; }
		public Panel MediumLoadouts { get; private set; }
		public Panel HeavyLoadouts { get; private set; }
		public LoadoutSelectItem Selected { get; private set; }
		public Action<BaseLoadout> OnLoadoutSelected { get; set; }

		public void Populate( Player player )
		{
			AddLoadouts( player, LoadoutArmorType.Light, LightLoadouts );
			AddLoadouts( player, LoadoutArmorType.Medium, MediumLoadouts );
			AddLoadouts( player, LoadoutArmorType.Heavy, HeavyLoadouts );
		}

		public void AddLoadouts( Player player, LoadoutArmorType armor, Panel container )
		{
			container.DeleteChildren();

			var loadouts = Library.GetAll<BaseLoadout>();

			foreach ( var type in loadouts )
			{
				if ( type == typeof( BaseLoadout ) )
					continue;

				var loadout = Library.Create<BaseLoadout>( type );

				if ( loadout.ArmorType != armor )
					continue;

				if ( loadout.UpgradesTo == null || !player.HasLoadoutUpgrade( loadout.UpgradesTo ) )
				{
					if ( loadout.UpgradeCost == 0 || player.HasLoadoutUpgrade( type ) )
					{
						var child = container.AddChild<LoadoutSelectItem>();
						child.SetLoadout( loadout );
						child.SetList( this );
					}
				}
			}

			container.SortChildren<LoadoutSelectItem>( ( panel ) =>
			{
				return panel.Loadout.DisplayOrder;
			} );

			foreach ( var item in container.ChildrenOfType<LoadoutSelectItem>() )
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
