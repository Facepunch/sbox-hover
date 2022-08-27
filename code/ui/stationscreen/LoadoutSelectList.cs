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

			var loadouts = TypeLibrary.GetDescriptions<BaseLoadout>();

			foreach ( var desc in loadouts )
			{
				if ( desc.TargetType == typeof( BaseLoadout ) )
					continue;

				var loadout = TypeLibrary.Create<BaseLoadout>( desc.TargetType );

				if ( loadout.ArmorType != armor )
					continue;

				if ( loadout.UpgradesTo == null || !player.HasLoadoutUpgrade( loadout.UpgradesTo ) )
				{
					if ( loadout.UpgradeCost == 0 || player.HasLoadoutUpgrade( desc.TargetType ) )
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

		protected override void PostTemplateApplied()
		{
			if ( Local.Pawn is Player player )
			{
				Populate( player );
			}

			base.PostTemplateApplied();
		}
	}
}
