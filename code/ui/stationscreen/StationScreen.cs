using Gamelib.Extensions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public enum StationScreenMode
	{
		Deployment,
		Station
	}

	[UseTemplate]
	public partial class StationScreen : Panel
	{
		public static StationScreen Instance { get; private set; }

		public struct LoadoutData
		{
			public string Name { get; set; }
			public string Weight { get; set; }
			public string Description { get; set; }
			public string Health { get; set; }
			public string Speed { get; set; }
			public string Energy { get; set; }
		}

		[ClientRpc]
		public static void Refresh()
		{
			if ( Instance.IsOpen )
			{

			}
		}

		[ClientRpc]
		public static void Toggle()
		{
			Instance.SetOpen( !Instance.IsOpen );
		}

		[ClientRpc]
		public static void Show( StationScreenMode mode )
		{
			Instance.SetMode( mode );
			Instance.SetOpen( true );
		}

		[ClientRpc]
		public static void Hide()
		{
			Instance.SetOpen( false );
		}

		public LoadoutSelectList LoadoutList { get; private set; }
		public StationScreenView CurrentView { get; private set; }
		public StationScreenMode Mode { get; private set; }
		public WeaponConfig CurrentWeapon { get; private set; }
		public WeaponConfig[] Weapons { get; private set; }
		public LoadoutData LoadoutInfo { get; set; }
		public Panel UpgradesList { get; private set; }
		public Image WeaponIcon { get; private set; }
		public string PlayerTokens => GetPlayerTokens();
		public BaseLoadout Loadout { get; private set; }
		public Panel WeaponList { get; private set; }
		public int CurrentSlot { get; private set; }
		public bool IsOpen { get; private set; }
		public string SlotName => Loadout.GetSlotName( CurrentSlot );

		public StationScreen()
		{
			SetClass( "hidden", true );

			Instance?.Delete();
			Instance = this;
		}

		public void SetWeapon( WeaponConfig config )
		{
			var items = WeaponList.ChildrenOfType<LoadoutWeaponListItem>();

			foreach ( var item in items )
			{
				item.IsActive = (item.Config.GetType() == config.GetType());
			}

			UpgradesList.DeleteChildren();

			if ( config.Upgrades != null )
			{
				for ( int i = 0; i < config.Upgrades.Count; i++ )
				{
					var upgrade = Library.Create<WeaponUpgrade>( config.Upgrades[i] );
					var item = UpgradesList.AddChild<LoadoutWeaponUpgradeItem>();
					item.SetUpgrade( i, config, upgrade );
				}
			}

			WeaponIcon.SetTexture( config.Icon );

			CurrentWeapon = config;
		}

		public void SetLoadout( BaseLoadout loadout )
		{
			if ( Local.Pawn is not Player player )
			{
				return;
			}

			LoadoutInfo = new LoadoutData
			{
				Name = loadout.Name,
				Description = loadout.Description,
				Energy = $"{loadout.Energy.CeilToInt()}",
				Health = $"{loadout.Health.CeilToInt()}",
				Speed = $"{loadout.MaxSpeed}m/s",
				Weight = loadout.ArmorType.ToString()
			};

			var configs = new WeaponConfig[loadout.AvailableWeapons.Length];

			// If we are this loadout, default to the weapons we have equipped.
			if ( player.Loadout.GetType() == loadout.GetType() )
			{
				foreach ( var weapon in player.Children.OfType<Weapon>() )
				{
					var slotToIndex = weapon.Slot - 1;

					if ( slotToIndex < configs.Length )
					{
						foreach ( var valid in loadout.AvailableWeapons[slotToIndex] )
						{
							if ( weapon.Config.Name == valid.Name )
							{
								configs[slotToIndex] = weapon.Config;
							}
						}
					}
				}
			}

			for ( var i = 0; i < configs.Length; i++ )
			{
				if ( configs[i] == null )
				{
					configs[i] = loadout.AvailableWeapons[i].FirstOrDefault();
				}
			}

			var weaponsList = this.GetAllChildrenOfType<LoadoutWeaponItem>();

			foreach ( var weapon in weaponsList )
			{
				var slot = weapon.Index;

				if ( configs.Length > slot )
					weapon.SetWeapon( loadout, configs[slot] );
				else
					weapon.SetWeapon( loadout, null );
			}

			Weapons = configs;
			Loadout = loadout;
		}

		public void SetOpen( bool isOpen )
		{
			if ( Local.Pawn is Player player )
			{
				SetClass( "hidden", !isOpen );
				IsOpen = isOpen;

				if ( isOpen )
				{
					LoadoutList.Populate( player );
				}
			}
		}

		public void SetMode( StationScreenMode mode )
		{
			Mode = mode;
		}

		public void SetView( string name )
		{
			foreach ( var view in ChildrenOfType<StationScreenView>() )
			{
				if ( view.Name == name )
				{
					CurrentView = view;
					view.SetActive( true );
				}
				else
				{
					view.SetActive( false );
				}
			}
		}

		public void DoSelectWeapon()
		{
			Weapons[CurrentSlot] = CurrentWeapon;

			var weaponsList = this.GetAllChildrenOfType<LoadoutWeaponItem>();

			foreach ( var weapon in weaponsList )
			{
				if ( weapon.Index == CurrentSlot )
				{
					weapon.SetWeapon( Loadout, CurrentWeapon );
				}
			}

			SetView( "loadout" );
		}

		public void OpenWeapons( int slot )
		{
			SetView( "weapon" );

			WeaponList.DeleteChildren();

			var weapons = Loadout.AvailableWeapons[slot];

			foreach ( var weapon in weapons )
			{
				var item = WeaponList.AddChild<LoadoutWeaponListItem>();
				item.SetConfig( weapon );
			}

			SetWeapon( Weapons[slot] );

			CurrentSlot = slot;
		}

		public void DoCancel()
		{
			Hide();
		}

		public void DoDeploy()
		{
			Hide();

			var loadout = LoadoutList.Selected.Loadout;
			var loadoutName = loadout.GetType().Name;
			var weapons = "";

			Player.ChangeLoadout( loadoutName, weapons );
		}

		protected string GetPlayerTokens()
		{
			if ( Local.Pawn is Player player )
				return player.Tokens.ToString();
			else
				return "0";
		}

		protected void OnLoadoutSelected( BaseLoadout loadout )
		{
			SetLoadout( loadout );
		}

		protected override void PostTemplateApplied()
		{
			LoadoutList.OnLoadoutSelected += OnLoadoutSelected;

			var view = ChildrenOfType<StationScreenView>()
				.Where( v => v.IsDefault )
				.FirstOrDefault();

			if ( view != null )
			{
				CurrentView = view;
				CurrentView.SetActive( true );
			}

			base.PostTemplateApplied();
		}
	}
}
