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
				if ( player.HasTokens( Loadout.TokenCost ) )
				{
					var primaryWeapon = PrimaryHolder?.Config?.Name ?? "";
					var secondaryWeapon = SecondaryHolder?.Config?.Name ?? "";

					StationScreen.Hide();
					Player.BuyLoadoutUpgrade( Loadout.GetType().Name, primaryWeapon, secondaryWeapon );
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
					LoadoutUpgrade.SetLoadout( loadout );
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
				if ( weapon.Upgrades != null )
				{
					foreach ( var upgradeType in weapon.Upgrades )
					{
						if ( player.HasWeaponUpgrade( weapon, upgradeType ) )
						{
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
		public Image Icon { get; set; }
		public StationScreenTextButton Button { get; set; }
		public WeaponConfig Weapon { get; set; }
		public Action<WeaponConfig> Callback { get; set; }

		public StationScreenWeaponInfo()
		{
			Name = Add.Label( "", "name" );
			Description = Add.Label( "", "description" );
			Icon = Add.Image( "", "icon" );
			Button = AddChild<StationScreenTextButton>( "button" );
			Button.SetText( "Select" );
			Button.OnClicked = OnSelected;
		}

		public void SetWeapon( WeaponConfig weapon, Action<WeaponConfig> callback )
		{
			Name.Text = weapon.Name;
			Description.Text = weapon.Description;
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

		public void SetWeapons( StationScreenWeapon holder, List<WeaponConfig> weapons )
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
		public WeaponConfig PrimaryConfig { get; private set; }
		public WeaponConfig SecondaryConfig { get; private set; }
		public StationScreenWeapon PrimaryHolder { get; private set; }
		public StationScreenWeapon SecondaryHolder { get; private set; }

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

		public void SetLoadout( BaseLoadout loadout )
		{
			if ( Local.Pawn is not Player player )
				return;

			Loadout = loadout;

			foreach ( var weapon in player.Children.OfType<Weapon>() )
			{
				var primaryMatch = loadout.PrimaryWeapons.Find( v => v.Name == weapon.Config.Name );

				if ( primaryMatch != null )
					PrimaryConfig = primaryMatch;

				var secondaryMatch = loadout.SecondaryWeapons.Find( v => v.Name == weapon.Config.Name );

				if ( secondaryMatch != null )
					PrimaryConfig = secondaryMatch;
			}

			PrimaryConfig ??= loadout.PrimaryWeapons.FirstOrDefault();
			SecondaryConfig ??= loadout.SecondaryWeapons.FirstOrDefault();

			var stationScreen = StationScreen.Instance;

			if ( PrimaryConfig  != null )
			{
				PrimaryHolder = WeaponsContainer.AddChild<StationScreenWeapon>( "weapon primary" );
				PrimaryHolder.SetConfig( PrimaryConfig );
				PrimaryHolder.OnClicked = () =>
				{
					stationScreen.WeaponSelector.SetWeapons( PrimaryHolder, loadout.PrimaryWeapons );
					stationScreen.WeaponSelector.Show( true );
				};
			}

			if ( SecondaryConfig != null )
			{
				SecondaryHolder = WeaponsContainer.AddChild<StationScreenWeapon>( "weapon secondary" );
				SecondaryHolder.SetConfig( SecondaryConfig );
				SecondaryHolder.OnClicked = () =>
				{
					stationScreen.WeaponSelector.SetWeapons( SecondaryHolder, loadout.SecondaryWeapons );
					stationScreen.WeaponSelector.Show( true );
				};
			}

			Title.Text = loadout.Name;
			Description.Text = loadout.Description;
			SecondaryDescription.Text = loadout.SecondaryDescription;

			if ( string.IsNullOrEmpty( loadout.SecondaryDescription ) )
				SecondaryDescription.SetClass( "hidden", true );
			else
				SecondaryDescription.SetClass( "hidden", false );

			BuyButton.SetAmount( loadout.TokenCost );

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
				BuyButton.SetClass( "affordable", player.HasTokens( Loadout.TokenCost ) );
			}

			base.Tick();
		}

		protected virtual void OnBuyClicked()
		{
			if ( Local.Pawn is Player player )
			{
				if ( player.HasTokens( Loadout.TokenCost ) )
				{
					var primaryWeapon = PrimaryHolder?.Config?.Name ?? "";
					var secondaryWeapon = SecondaryHolder?.Config?.Name ?? "";

					StationScreen.Hide();
					Player.BuyLoadout( Loadout.GetType().Name, primaryWeapon, secondaryWeapon );
					Audio.Play( "hover.clickbeep" );
				}
			}
		}
	}

	public partial class StationScreenLoadouts : StationScreenTabContent
	{
		public List<StationScreenLoadout> Loadouts { get; private set; } = new();

		public StationScreenLoadouts()
		{

		}

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
						var child = AddChild<StationScreenLoadout>();
						child.SetLoadout( loadout );
						Loadouts.Add( child );
					}
				}

				if ( loadout.Health > maxHealth ) maxHealth = loadout.Health;
				if ( loadout.Energy > maxEnergy ) maxEnergy = loadout.Energy;
				if ( loadout.MaxSpeed > maxSpeed ) maxSpeed = loadout.MaxSpeed;
			}

			SortChildren<StationScreenLoadout>( ( panel ) =>
			{
				return panel.Loadout.DisplayOrder;
			} );

			foreach ( var loadout in Loadouts )
			{
				loadout.UpdateBars( maxHealth, maxEnergy, maxSpeed );
			}

			base.Initialize();
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
		public static void Show()
		{
			Instance.SetOpen( true );
		}

		[ClientRpc]
		public static void Hide()
		{
			Instance.SetOpen( false );
		}

		public StationScreenWeaponSelector WeaponSelector { get; private set; }
		public StationScreenTabList TabList { get; private set; }
		public Panel ContentContainer { get; private set; }
		public Label CloseTip { get; private set; }

		public bool IsOpen { get; private set; }

		public void SetOpen( bool isOpen )
		{
			if ( isOpen ) InitializeContent();
			SetClass( "hidden", !isOpen );
			IsOpen = isOpen;
		}

		public StationScreen()
		{
			StyleSheet.Load( "/ui/StationScreen.scss" );

			TabList = AddChild<StationScreenTabList>( "tabs" );
			CloseTip = Add.Label( $"Press [{Input.GetKeyWithBinding( "iv_use" )}] to Exit Station", "close" );
			ContentContainer = Add.Panel( "content" );

			var loadoutsContent = ContentContainer.AddChild<StationScreenLoadouts>( "loadouts" );
			loadoutsContent.Initialize();

			var upgradesContent = ContentContainer.AddChild<StationScreenUpgrades>( "upgrades" );
			upgradesContent.Initialize();

			var loadoutsTab = new StationScreenTab();
			loadoutsTab.Setup( "Loadouts", "ui/icons/loadouts.png", loadoutsContent );

			var upgradesTab = new StationScreenTab();
			upgradesTab.Setup( "Upgrades", "ui/icons/upgrades.png", upgradesContent );

			TabList.AddTab( "loadouts", loadoutsTab );
			TabList.AddTab( "upgrades", upgradesTab );

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
