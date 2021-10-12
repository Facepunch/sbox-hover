using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
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
			Amount.Text = amount.ToString();
		}

		protected override void OnClick( MousePanelEvent e )
		{
			OnClicked?.Invoke();
			base.OnClick( e );
		}
	}

	public partial class StationScreenLoadout : Panel
	{
		public List<AnimSceneObject> SceneObjects { get; private set; } = new();
		public Label Title { get; private set; }
		public Label Description { get; private set; }
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

		public StationScreenLoadout()
		{
			SceneWorld = new SceneWorld();
			Scene = Add.ScenePanel( SceneWorld, Vector3.Zero, Rotation.Identity, 30f, "scene" );

			Title = Add.Label( "", "title" );
			Description = Add.Label( "", "description" );

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
			Loadout = loadout;

			foreach ( var icon in loadout.WeaponIcons )
			{
				WeaponsContainer.Add.Image( icon, "icon" );
			}

			Title.Text = loadout.Name;
			Description.Text = loadout.Description;

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

				// TODO: Can't seem to get the weapon model to display.

				/*
				var weapon = new AnimSceneObject( Model.Load( loadout.DisplayWeapon ), Avatar.Transform );
				Avatar.AddChild( "clothing", weapon );
				Avatar.SetAnimInt( "holdtype", 2 );
				SceneObjects.Add( weapon );
				*/

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

		private void OnBuyClicked()
		{
			if ( Local.Pawn is Player player )
			{
				if ( player.HasTokens( Loadout.TokenCost ) )
				{
					StationScreen.Hide();
					Player.BuyLoadout( Loadout.GetType().Name );
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
			var loadouts = Library.GetAll<BaseLoadout>();
			var maxHealth = 0f;
			var maxEnergy = 0f;
			var maxSpeed = 0f;

			foreach ( var type in loadouts )
			{
				if ( type == typeof( BaseLoadout ) )
					continue;

				var loadout = Library.Create<BaseLoadout>( type );
				var child = AddChild<StationScreenLoadout>();
				child.SetLoadout( loadout );
				Loadouts.Add( child );

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
		}

		public override void Show()
		{


			base.Show();
		}
	}

	public partial class StationScreenTabContent : Panel
	{
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
			OnClicked?.Invoke();
			base.OnClick( e );
		}

		public void Setup( string title, string icon, StationScreenTabContent content )
		{
			Content = content;
			Title.Text = title;
			Icon.SetTexture( icon );
		}

		public override void Tick()
		{
			SetClass( "selected", IsSelected );
			base.Tick();
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
			if ( SelectedTab == null )
			{
				Select( tab );
			}

			tab.AddClass( className );
			tab.OnClicked = () =>
			{
				Audio.Play( "hover.clickbeep" );
				Select( tab );
			};

			AddChild( tab );
		}
	}

	public partial class StationScreen : Panel
	{
		public static StationScreen Instance { get; private set; }

		[ClientRpc]
		public static void Toggle()
		{
			Instance.SetClass( "hidden", !Instance.HasClass( "hidden" ) );
		}

		[ClientRpc]
		public static void Show()
		{
			Instance.SetClass( "hidden", false );
		}

		[ClientRpc]
		public static void Hide()
		{
			Instance.SetClass( "hidden", true );
		}

		public StationScreenTabList TabList { get; private set; }
		public Panel ContentContainer { get; private set; }
		public Label CloseTip { get; private set; }

		public StationScreen()
		{
			StyleSheet.Load( "/ui/StationScreen.scss" );

			TabList = AddChild<StationScreenTabList>( "tabs" );
			CloseTip = Add.Label( $"Press [{Input.GetKeyWithBinding( "iv_use" )}] to Exit Station", "close" );
			ContentContainer = Add.Panel( "content" );

			var loadoutsContent = ContentContainer.AddChild<StationScreenLoadouts>( "loadouts" );
			var upgradesContent = ContentContainer.AddChild<StationScreenTabContent>( "upgrades" );

			var loadoutsTab = new StationScreenTab();
			loadoutsTab.Setup( "Loadouts", "ui/icons/buzzkill.png", loadoutsContent );

			var upgradesTab = new StationScreenTab();
			upgradesTab.Setup( "Upgrades", "ui/icons/buzzkill.png", upgradesContent );

			TabList.AddTab( "loadouts", loadoutsTab );
			TabList.AddTab( "upgrades", upgradesTab );

			SetClass( "hidden", true );

			Instance = this;
		}

		public override void Tick()
		{
			base.Tick();
		}
	}
}
