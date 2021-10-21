using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class StationScreenLoadoutUpgrade : StationScreenLoadout
	{
		protected override void OnBuyClicked()
		{
			if ( Local.Pawn is Player player )
			{
				if ( player.HasTokens( Loadout.UpgradeCost ) )
				{
					var weapons = new string[Holders.Length];

					for ( int i = 0; i < Holders.Length; i++ )
					{
						var holder = Holders[i];
						weapons[i] = holder.Config.Name;
					}

					StationScreen.Hide();
					Player.BuyLoadoutUpgrade( Loadout.GetType().Name, string.Join( ',', weapons ) );
					Audio.Play( "hover.clickbeep" );
				}
			}
		}
	}

	public partial class StationScreenWeaponUpgrade : Panel
	{
		public Label Title { get; private set; }
		public Label Description { get; private set; }
		public Image Weapon { get; private set; }
		public StationScreenBuyButton BuyButton { get; private set; }
		public string WeaponName { get; private set; }
		public WeaponUpgrade Upgrade { get; private set; }

		public StationScreenWeaponUpgrade()
		{
			Title = Add.Label( "", "title" );
			Description = Add.Label( "", "description" );
			Weapon = Add.Image( "", "weapon" );

			BuyButton = AddChild<StationScreenBuyButton>( "buy" );
			BuyButton.OnClicked = OnBuyClicked;
		}

		public void SetUpgrade( Weapon weapon, WeaponUpgrade upgrade )
		{
			WeaponName = weapon.Name;
			Upgrade = upgrade;

			Title.Text = upgrade.Name;
			Weapon.Texture = Texture.Load( weapon.Config.Icon );
			Description.Text = upgrade.Description;

			BuyButton.SetAmount( upgrade.TokenCost );
		}

		public override void Tick()
		{
			if ( Local.Pawn is Player player )
			{
				BuyButton.SetClass( "affordable", player.HasTokens( Upgrade.TokenCost ) );
			}

			base.Tick();
		}

		protected virtual void OnBuyClicked()
		{
			if ( Local.Pawn is Player player )
			{
				if ( player.HasTokens( Upgrade.TokenCost ) )
				{
					Player.BuyWeaponUpgrade( WeaponName, Upgrade.GetType().Name );
					Audio.Play( "hover.clickbeep" );
				}
			}
		}
	}

	public partial class StationScreenUpgrades : StationScreenTabContent
	{
		public List<StationScreenWeaponUpgrade> WeaponUpgrades { get; private set; }
		public StationScreenLoadoutUpgrade LoadoutUpgrade { get; private set; }
		public Panel WeaponContainer { get; private set; }

		private bool HasUpgrades { get; set; }

		public StationScreenUpgrades()
		{
			WeaponUpgrades = new();
		}

		public override bool IsAvailable()
		{
			if ( StationScreen.Instance.Mode == StationScreenMode.Deployment )
				return false;

			return HasUpgrades;
		}

		public override void Initialize()
		{
			if ( Local.Pawn is not Player player )
				return;

			if ( LoadoutUpgrade != null )
			{
				LoadoutUpgrade.Delete( true );
				LoadoutUpgrade = null;
			}

			WeaponContainer?.Delete();
			HasUpgrades = false;

			foreach ( var upgrade in WeaponUpgrades )
			{
				upgrade.Delete( true );
			}

			WeaponUpgrades.Clear();

			var loadoutUpgradeType = player.Loadout.UpgradesTo;
			var loadouts = Library.GetAll<BaseLoadout>();
			var maxHealth = 0f;
			var maxEnergy = 0f;
			var maxSpeed = 0f;

			foreach ( var type in loadouts )
			{
				if ( type == typeof( BaseLoadout ) )
					continue;

				var loadout = Library.Create<BaseLoadout>( type );

				if ( type == loadoutUpgradeType )
				{
					LoadoutUpgrade = AddChild<StationScreenLoadoutUpgrade>();
					LoadoutUpgrade.SetLoadout( loadout, loadout.UpgradeCost );
					HasUpgrades = true;
				}

				if ( loadout.Health > maxHealth ) maxHealth = loadout.Health;
				if ( loadout.Energy > maxEnergy ) maxEnergy = loadout.Energy;
				if ( loadout.MaxSpeed > maxSpeed ) maxSpeed = loadout.MaxSpeed;
			}

			if ( LoadoutUpgrade != null )
			{
				LoadoutUpgrade.UpdateBars( maxHealth, maxEnergy, maxSpeed );
			}

			WeaponContainer = Add.Panel( "weapons" );

			var weapons = player.Children.OfType<Weapon>();

			foreach ( var weapon in weapons )
			{
				if ( weapon.Upgrades == null ) continue;

				var ownedUpgrades = player.GetWeaponUpgrades( weapon );

				for ( var i = 0; i < weapon.Upgrades.Count; i++ )
				{
					var upgradeType = weapon.Upgrades[i];

					if ( ownedUpgrades != null )
					{
						var ownedUpgrade = ownedUpgrades.ElementAtOrDefault( i );

						if ( ownedUpgrade != null && ownedUpgrade.GetType() == upgradeType )
							continue;
					}

					var upgrade = Library.Create<WeaponUpgrade>( upgradeType );

					var child = WeaponContainer.AddChild<StationScreenWeaponUpgrade>();
					child.SetUpgrade( weapon, upgrade );

					WeaponUpgrades.Add( child );
					HasUpgrades = true;

					break;
				}
			}

			base.Initialize();
		}
	}

	public partial class StationScreenTextButton : Panel
	{
		public Label Label { get; private set; }
		public Action OnClicked { get; set; }

		public StationScreenTextButton()
		{
			Label = Add.Label( "", "label" );
		}

		public void SetText( string text )
		{
			Label.Text = text;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			OnClicked?.Invoke();
			base.OnClick( e );
		}
	}

	public partial class StationScreenBuyButton : Panel
	{
		public Image Icon { get; private set; }
		public Label Amount { get; private set; }
		public Action OnClicked { get; set; }

		public StationScreenBuyButton()
		{
			Icon = Add.Image( "ui/icons/tokens.png", "icon" );
			Amount = Add.Label( "", "amount" );
		}

		public void SetAmount( int amount )
		{
			Amount.Text = $"{amount:C0}";
		}

		public void SetText( string text )
		{
			Amount.Text = text;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			OnClicked?.Invoke();
			base.OnClick( e );
		}
	}

	public partial class StationScreenWeapon : Panel
	{
		public Image Icon { get; private set; }
		public Action OnClicked { get; set; }
		public WeaponConfig Config { get; set; }

		public StationScreenWeapon()
		{
			Icon = Add.Image( "", "icon" );
		}

		public void SetConfig( WeaponConfig config )
		{
			Icon.SetTexture( config.Icon );
			Config = config;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			OnClicked?.Invoke();
			base.OnClick( e );
		}
	}

	public partial class StationScreenWeaponInfo : Panel
	{
		public Label Name { get; set; }
		public Label Description { get; set; }
		public Label SecondaryDescription { get; set; }
		public Image Icon { get; set; }
		public StationScreenTextButton Button { get; set; }
		public WeaponConfig Weapon { get; set; }
		public Action<WeaponConfig> Callback { get; set; }

		public StationScreenWeaponInfo()
		{
			Name = Add.Label( "", "name" );
			Description = Add.Label( "", "description" );
			SecondaryDescription = Add.Label( "", "secondary_description" );
			Icon = Add.Image( "", "icon" );
			Button = AddChild<StationScreenTextButton>( "button" );
			Button.SetText( "Select" );
			Button.OnClicked = OnSelected;
		}

		public void SetWeapon( WeaponConfig weapon, Action<WeaponConfig> callback )
		{
			Name.Text = weapon.Name;
			Description.Text = weapon.Description;

			if ( !string.IsNullOrEmpty( weapon.SecondaryDescription ) )
			{
				SecondaryDescription.SetClass( "hidden", false );
				SecondaryDescription.Text = weapon.SecondaryDescription;
			}
			else
			{
				SecondaryDescription.SetClass( "hidden", true );
			}

			Icon.SetTexture( weapon.Icon );
			Callback = callback;
			Weapon = weapon;
		}

		private void OnSelected()
		{
			Callback?.Invoke( Weapon );
		}
	}

	public partial class StationScreenWeaponSelector : Panel
	{
		public List<StationScreenWeaponInfo> WeaponInfo { get; set; }
		public StationScreenWeapon WeaponHolder { get; set; }
		public Panel Container { get; set; }

		public StationScreenWeaponSelector()
		{
			WeaponInfo = new();
			Container = Add.Panel( "container" );
		}

		public void SetWeapons( StationScreenWeapon holder, WeaponConfig[] weapons )
		{
			foreach ( var weapon in WeaponInfo )
			{
				weapon.Delete( true );
			}

			WeaponHolder = holder;
			WeaponInfo.Clear();

			foreach ( var weapon in weapons )
			{
				var item = Container.AddChild<StationScreenWeaponInfo>( "weapon" );
				item.SetWeapon( weapon, OnWeaponSelected );
				WeaponInfo.Add( item );
			}
		}

		public void Show( bool shouldShow )
		{
			SetClass( "hidden", !shouldShow );
		}

		private void OnWeaponSelected( WeaponConfig config )
		{
			WeaponHolder.SetConfig( config );
			Show( false );
		}
	}

	public partial class StationScreenLoadout : Panel
	{
		public List<AnimSceneObject> SceneObjects { get; private set; } = new();
		public Label Title { get; private set; }
		public Label Description { get; private set; }
		public Label SecondaryDescription { get; private set; }
		public SimpleIconBar HealthBar { get; private set; }
		public SimpleIconBar EnergyBar { get; private set; }
		public SimpleIconBar SpeedBar { get; private set; }
		public AnimSceneObject Avatar { get; private set; }
		public SceneWorld SceneWorld { get; private set; }
		public ScenePanel Scene { get; private set; }
		public Panel WeaponsContainer { get; private set; }
		public Panel StatsContainer { get; private set; }
		public StationScreenBuyButton BuyButton { get; private set; }
		public BaseLoadout Loadout { get; private set; }
		public WeaponConfig[] Configs { get; private set; }
		public StationScreenWeapon[] Holders { get; private set; }
		public int Cost { get; private set; }

		public StationScreenLoadout()
		{
			SceneWorld = new SceneWorld();
			Scene = Add.ScenePanel( SceneWorld, Vector3.Zero, Rotation.Identity, 30f, "scene" );

			Title = Add.Label( "", "title" );
			Description = Add.Label( "", "description" );
			SecondaryDescription = Add.Label( "", "secondary_description" );

			StatsContainer = Add.Panel( "stats" );
			HealthBar = StatsContainer.AddChild<SimpleIconBar>( "health" );
			EnergyBar = StatsContainer.AddChild<SimpleIconBar>( "energy" );
			SpeedBar = StatsContainer.AddChild<SimpleIconBar>( "speed" );

			WeaponsContainer = Add.Panel( "weapons" );

			BuyButton = AddChild<StationScreenBuyButton>( "buy" );
			BuyButton.OnClicked = OnBuyClicked;
		}

		public void UpdateBars( float maxHealth, float maxEnergy, float maxSpeed )
		{
			UpdateBar( HealthBar, Loadout.Health, maxHealth );
			UpdateBar( EnergyBar, Loadout.Energy, maxEnergy );
			UpdateBar( SpeedBar, Loadout.MaxSpeed, maxSpeed );
		}

		public void SetLoadout( BaseLoadout loadout, int cost )
		{
			if ( Local.Pawn is not Player player )
				return;

			Loadout = loadout;
			Configs = new WeaponConfig[loadout.AvailableWeapons.Length];
			Holders = new StationScreenWeapon[loadout.AvailableWeapons.Length];

			// If we are this loadout, default to the weapons we have equipped.
			if ( player.Loadout.GetType() == loadout.GetType() )
			{
				foreach ( var weapon in player.Children.OfType<Weapon>() )
				{
					var slotToIndex = weapon.Slot - 1;

					if ( slotToIndex < Configs.Length )
					{
						foreach ( var valid in loadout.AvailableWeapons[slotToIndex] )
						{
							if ( weapon.Config.Name == valid.Name )
							{
								Configs[slotToIndex] = weapon.Config;
							}
						}
					}
				}
			}

			for ( var i = 0; i < Configs.Length; i++ )
			{
				if ( Configs[i] == null )
				{
					Configs[i] = loadout.AvailableWeapons[i].FirstOrDefault();
				}
			}

			var stationScreen = StationScreen.Instance;

			for ( var i = 0; i < Configs.Length; i++ )
			{
				var config = Configs[i];
				var holder = WeaponsContainer.AddChild<StationScreenWeapon>( "weapon" );
				var index = i;
				holder.SetConfig( config );
				holder.OnClicked = () =>
				{
					stationScreen.WeaponSelector.SetWeapons( holder, loadout.AvailableWeapons[index] );
					stationScreen.WeaponSelector.Show( true );
				};
				Holders[i] = holder;
			}

			Title.Text = loadout.Name;
			Description.Text = loadout.Description;
			SecondaryDescription.Text = loadout.SecondaryDescription;

			if ( string.IsNullOrEmpty( loadout.SecondaryDescription ) )
				SecondaryDescription.SetClass( "hidden", true );
			else
				SecondaryDescription.SetClass( "hidden", false );

			if ( cost == 0 )
				BuyButton.SetText( "Select" );
			else
				BuyButton.SetAmount( cost );

			Cost = cost;

			using ( SceneWorld.SetCurrent( SceneWorld ) )
			{
				var model = Model.Load( "models/citizen/citizen.vmdl" );

				Avatar = new AnimSceneObject( model, Transform.Zero );

				foreach ( var clothing in loadout.Clothing )
				{
					var clothes = new AnimSceneObject( Model.Load( clothing ), Avatar.Transform );
					Avatar.AddChild( "clothing", clothes );
					SceneObjects.Add( clothes );
				}

				var lightWarm = new SpotLight( Vector3.Up * 100f + Vector3.Forward * 100f + Vector3.Right * -200f, new Color( 1f, 0.95f, 0.8f ) * 60f );
				lightWarm.Rotation = Rotation.LookAt( -lightWarm.Position );
				lightWarm.SpotCone = new SpotLightCone { Inner = 90, Outer = 90 };

				var lightBlue = new SpotLight( Vector3.Up * 100f + Vector3.Forward * -100f + Vector3.Right * 100f, new Color( 0f, 0.4f, 1f ) * 100f );
				lightBlue.Rotation = Rotation.LookAt( -lightBlue.Position );
				lightBlue.SpotCone = new SpotLightCone { Inner = 90f, Outer = 90f };

				var angles = new Angles( 0f, 180f, 0f );
				var position = Vector3.Up * 40f + angles.Direction * -100f;

				Scene.World = SceneWorld;
				Scene.CameraPosition = position;
				Scene.CameraRotation = Rotation.From( angles );
				Scene.FieldOfView = 30f;
				Scene.AmbientColor = Color.Gray * 0.2f;

				SceneObjects.Add( Avatar );
			}

			foreach ( var sceneObject in SceneObjects )
			{
				sceneObject.Update( RealTime.Delta );
			}
		}

		private void UpdateBar( SimpleIconBar bar, float value, float maximum )
		{
			var fraction = Length.Fraction( value / maximum );

			if ( bar.InnerBar.Style.Width != fraction )
			{
				bar.InnerBar.Style.Width = fraction;
				bar.InnerBar.Style.Dirty();
			}
		}

		public override void Tick()
		{
			foreach ( var sceneObject in SceneObjects )
			{
				sceneObject.Update( RealTime.Delta );
			}

			if ( Loadout != null && Local.Pawn is Player player )
			{
				var currentLoadout = player.Loadout;
				SetClass( "selected", currentLoadout != null && currentLoadout.GetType() == Loadout.GetType() );
				BuyButton.SetClass( "affordable", player.HasTokens( Cost ) );
			}

			base.Tick();
		}

		protected virtual void OnBuyClicked()
		{
			if ( Local.Pawn is Player player )
			{
				if ( player.HasTokens( Loadout.TokenCost ) )
				{
					var weapons = new string[Holders.Length];

					for ( int i = 0; i < Holders.Length; i++ )
					{
						var holder = Holders[i];
						weapons[i] = holder.Config.Name;
					}

					StationScreen.Hide();
					Player.BuyLoadout( Loadout.GetType().Name, string.Join( ',', weapons ) );
					Audio.Play( "hover.clickbeep" );
				}
			}
		}
	}

	public partial class StationScreenLoadouts : StationScreenTabContent
	{
		public List<StationScreenLoadout> Loadouts { get; private set; } = new();

		public StationScreenTextButton PreviousButton { get; private set; }
		public StationScreenTextButton NextButton { get; private set; }
		public Panel Container { get; private set; }

		public StationScreenLoadouts()
		{
			PreviousButton = AddChild<StationScreenTextButton>( "previous" );
			PreviousButton.SetText( "<" );
			PreviousButton.OnClicked += OnPreviousPage;

			Container = Add.Panel( "container" );

			NextButton = AddChild<StationScreenTextButton>( "next" );
			NextButton.SetText( ">" );
			NextButton.OnClicked += OnNextPage;
		}

		public int ItemsPerPage { get; set; } = 3;
		public int PageIndex { get; private set; }

		public override void Initialize()
		{
			if ( Local.Pawn is not Player player )
				return;

			foreach ( var loadout in Loadouts )
			{
				loadout.Delete( true );
			}

			Loadouts.Clear();

			var loadouts = Library.GetAll<BaseLoadout>();
			var maxHealth = 0f;
			var maxEnergy = 0f;
			var maxSpeed = 0f;

			foreach ( var type in loadouts )
			{
				if ( type == typeof( BaseLoadout ) )
					continue;

				var loadout = Library.Create<BaseLoadout>( type );

				if ( loadout.UpgradesTo == null || !player.HasLoadoutUpgrade( loadout.UpgradesTo ) )
				{
					if ( loadout.UpgradeCost == 0 || player.HasLoadoutUpgrade( type ) )
					{
						var child = Container.AddChild<StationScreenLoadout>();
						child.SetLoadout( loadout, loadout.TokenCost );
						child.SetClass( "hidden", true );
						Loadouts.Add( child );
					}
				}

				if ( loadout.Health > maxHealth ) maxHealth = loadout.Health;
				if ( loadout.Energy > maxEnergy ) maxEnergy = loadout.Energy;
				if ( loadout.MaxSpeed > maxSpeed ) maxSpeed = loadout.MaxSpeed;
			}

			Container.SortChildren<StationScreenLoadout>( ( panel ) =>
			{
				return panel.Loadout.DisplayOrder;
			} );

			Loadouts.Sort( ( a, b ) => a.Loadout.DisplayOrder.CompareTo( b.Loadout.DisplayOrder ) );

			foreach ( var loadout in Loadouts )
			{
				loadout.UpdateBars( maxHealth, maxEnergy, maxSpeed );
			}

			UpdatePageItems();

			base.Initialize();
		}

		private void UpdatePageItems()
		{
			foreach ( var loadout in Loadouts )
			{
				loadout.SetClass( "hidden", true );
			}

			for ( var i = PageIndex; i < PageIndex + ItemsPerPage; i++ )
			{
				if ( i < Loadouts.Count )
				{
					var loadout = Loadouts[i];
					loadout.SetClass( "hidden", false );
				}
			}

			PreviousButton.SetClass( "disabled", PageIndex == 0 );
			NextButton.SetClass( "disabled", PageIndex >= Loadouts.Count - ItemsPerPage );
		}

		private void OnPreviousPage()
		{
			if ( PageIndex == 0 )
				return;

			PageIndex = Math.Max( PageIndex - ItemsPerPage, 0 );
			UpdatePageItems();
			Audio.Play( "hover.clickbeep" );
		}

		private void OnNextPage()
		{
			if ( PageIndex >= Loadouts.Count - ItemsPerPage )
				return;

			PageIndex = Math.Min( PageIndex + ItemsPerPage, Loadouts.Count - ItemsPerPage );
			UpdatePageItems();
			Audio.Play( "hover.clickbeep" );
		}
	}

	public partial class StationScreenTabContent : Panel
	{
		public virtual bool IsAvailable()
		{
			return true;
		}


		public virtual void Initialize()
		{

		}

		public virtual void Show()
		{
			SetClass( "hidden", false );
		}

		public virtual void Hide()
		{
			SetClass( "hidden", true );
		}
	}

	public partial class StationScreenTab : Panel
	{
		public Image Icon { get; private set; }
		public Label Title { get; private set; }
		public bool IsSelected { get; set; }
		public Action OnClicked { get; set; }
		public StationScreenTabContent Content { get; set; }

		public StationScreenTab()
		{
			Icon = Add.Image( "", "icon" );
			Title = Add.Label( "", "title" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			if ( IsAvailable() )
			{
				OnClicked?.Invoke();
			}

			base.OnClick( e );
		}

		public virtual bool IsAvailable()
		{
			return Content.IsAvailable();
		}

		public override void Tick()
		{
			var isAvailable = IsAvailable();
			SetClass( "selected", isAvailable && IsSelected );
			SetClass( "hidden", !isAvailable );

			base.Tick();
		}

		public void Setup( string title, string icon, StationScreenTabContent content )
		{
			Content = content;
			Title.Text = title;
			Icon.SetTexture( icon );
		}
	}

	public partial class StationScreenTabList : Panel
	{
		public List<StationScreenTab> Tabs { get; private set; } = new();
		public StationScreenTab SelectedTab { get; private set; }

		public StationScreenTabList()
		{

		}

		public void Select( StationScreenTab tab )
		{
			if ( SelectedTab == tab )
				return;

			if ( SelectedTab != null )
			{
				SelectedTab.IsSelected = false;
				SelectedTab.Content.Hide();
			}

			SelectedTab = tab;
			SelectedTab.IsSelected = true;
			SelectedTab.Content.Show();
		}

		public void AddTab( string className, StationScreenTab tab )
		{
			tab.AddClass( className );
			tab.OnClicked = () =>
			{
				Audio.Play( "hover.clickbeep" );
				Select( tab );
			};

			AddChild( tab );

			Tabs.Add( tab );

			if ( SelectedTab == null )
			{
				Select( tab );
				return;
			}

			tab.Content.Hide();
		}

		public override void Tick()
		{
			if ( SelectedTab != null && !SelectedTab.IsAvailable() )
			{
				foreach ( var tab in Tabs )
				{
					if ( tab.IsAvailable() )
					{
						Select( tab );
						break;
					}
				}
			}

			base.Tick();
		}
	}

	public enum StationScreenMode
	{
		Deployment,
		Station
	}

	public partial class StationScreen : Panel
	{
		public static StationScreen Instance { get; private set; }

		[ClientRpc]
		public static void Refresh()
		{
			if ( Instance.IsOpen )
			{
				Instance.InitializeContent();
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

		public StationScreenWeaponSelector WeaponSelector { get; private set; }
		public StationScreenTabList TabList { get; private set; }
		public StationScreenTab LoadoutsTab { get; private set; }
		public StationScreenTab UpgradesTab { get; private set; }
		public Panel ContentContainer { get; private set; }
		public StationScreenMode Mode { get; private set; }
		public Panel TipContainer { get; private set; }
		public Label SecondaryTip { get; private set; }
		public Label PrimaryTip { get; private set; }
		public bool IsOpen { get; private set; }

		public void SetOpen( bool isOpen )
		{
			if ( isOpen ) InitializeContent();
			SetClass( "hidden", !isOpen );
			IsOpen = isOpen;
		}

		public void SetMode( StationScreenMode mode )
		{
			PrimaryTip.Text = "Change Your Loadout";

			if ( mode == StationScreenMode.Station )
				SecondaryTip.Text = $"Press [{Input.GetKeyWithBinding( "iv_use" )}] to Exit Station";
			else
				SecondaryTip.Text = "Visit a Station to Change Loadout in Future";

			if ( mode == StationScreenMode.Deployment )
			{
				TabList.Select( LoadoutsTab );
			}

			Mode = mode;
		}

		public StationScreen()
		{
			StyleSheet.Load( "/ui/StationScreen.scss" );

			TabList = AddChild<StationScreenTabList>( "tabs" );

			TipContainer = Add.Panel( "tips" );
			PrimaryTip = TipContainer.Add.Label( "", "primary" );
			SecondaryTip = TipContainer.Add.Label( "", "secondary" );
			ContentContainer = Add.Panel( "content" );

			var loadoutsContent = ContentContainer.AddChild<StationScreenLoadouts>( "loadouts" );
			loadoutsContent.Initialize();

			var upgradesContent = ContentContainer.AddChild<StationScreenUpgrades>( "upgrades" );
			upgradesContent.Initialize();

			LoadoutsTab = new StationScreenTab();
			LoadoutsTab.Setup( "Loadouts", "ui/icons/loadouts.png", loadoutsContent );

			UpgradesTab = new StationScreenTab();
			UpgradesTab.Setup( "Upgrades", "ui/icons/upgrades.png", upgradesContent );

			TabList.AddTab( "loadouts", LoadoutsTab );
			TabList.AddTab( "upgrades", UpgradesTab );

			WeaponSelector = AddChild<StationScreenWeaponSelector>( "selector" );
			WeaponSelector.Show( false );

			SetOpen( false );

			Instance = this;
		}

		public override void Tick()
		{
			base.Tick();
		}

		private void InitializeContent()
		{
			foreach ( var tab in TabList.Tabs )
			{
				tab.Content?.Initialize();
			}
		}
	}
}
