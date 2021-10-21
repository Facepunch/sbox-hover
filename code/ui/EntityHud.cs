using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class EntityHudIconList : Panel
	{
		public void SetActive( bool active )
		{
			SetClass( "hidden", !active );
		}
	}

	public class EntityHudIcon : Image
	{
		public void SetActive( bool active )
		{
			SetClass( "hidden", !active );
		}
	}

	public class EntityHudIconBar : Panel
	{
		public EntityHudIcon Icon { get; private set; }
		public EntityHudBar Bar { get; private set; }

		public EntityHudIconBar()
		{
			Icon = AddChild<EntityHudIcon>( "icon" );
			Bar = AddChild<EntityHudBar>( "bar" );
		}

		public void SetActive( bool active )
		{
			SetClass( "hidden", !active );
		}
	}

	public class OutpostHud : Panel
	{
		public Panel Container { get; private set; }
		public Label Letter { get; private set; }
		public Label Name { get; private set; }
		public Panel Bar { get; private set; }
		public OutpostVolume Outpost { get; private set; }

		public OutpostHud()
		{
			Container = Add.Panel( "container" );
			Bar = Container.Add.Panel( "bar" );

			var content = Container.Add.Panel( "content" );
			var circle = content.Add.Panel( "circle" );
			Letter = circle.Add.Label( "", "letter" );

			Name = content.Add.Label( "", "name" );
		}

		public void SetOutpost( OutpostVolume outpost )
		{
			Letter.Text = outpost.Letter;

			/*
			if ( !string.IsNullOrEmpty( outpost.OutpostName ) )
				Name.Text = outpost.OutpostName;
			else
				Name.SetClass( "hidden", true );
			*/

			Name.SetClass( "hidden", true );

			Outpost = outpost;
		}

		public override void Tick()
		{
			SetClass( Team.Blue.GetHudClass(), Outpost.Team == Team.Blue );
			SetClass( Team.Red.GetHudClass(), Outpost.Team == Team.Red );
			SetClass( Team.None.GetHudClass(), Outpost.Team == Team.None );

			if ( Outpost.CaptureProgress >= 0f )
			{
				Bar.SetClass( "hidden", false );
				Bar.Style.Width = Length.Fraction( Outpost.CaptureProgress );
			}
			else
			{
				Bar.SetClass( "hidden", true );
			}

			base.Tick();
		}
	}

	public class EntityHudBar : Panel
	{
		public Panel Foreground { get; private set; }

		public EntityHudBar()
		{
			Foreground = Add.Panel( "foreground" );
		}

		public void SetColor( Color color )
		{
			Foreground.Style.BackgroundColor = color;
		}

		public void SetProgress( float value )
		{
			var fraction = Length.Fraction( value );

			if ( Foreground.Style.Width != fraction )
			{
				Foreground.Style.Width = fraction;
				Foreground.Style.Dirty();
			}
		}

		public void SetActive( bool active )
		{
			SetClass( "hidden", !active );
		}
	}

	public class EntityHudAnchor : Panel
	{
		public IHudEntity Entity { get; private set; }
		public float UpOffset { get; set; } = 80f;
		public bool IsActive { get; private set; } = true;

		public void SetEntity( IHudEntity entity )
		{
			Entity = entity;
		}

		public void SetActive( bool active )
		{
			if ( IsActive != active )
			{
				IsActive = active;
				SetClass( "hidden", !active );
				if ( active ) UpdatePosition();
			}
		}

		public void UpdatePosition()
		{
			var position = (Entity.Position + Entity.LocalCenter + Vector3.Up * UpOffset).ToScreen();

			if ( position.z < 0 )
			{
				SetClass( "hidden", true );
				return;
			}

			Style.Left = Length.Fraction( position.x );
			Style.Top = Length.Fraction( position.y );
			Style.Dirty();

			SetClass( "hidden", !IsActive );
		}

		public override void Tick()
		{
			if ( (Entity as Entity).IsValid() )
			{
				if ( Entity.ShouldUpdateHud() )
				{
					Entity.UpdateHudComponents();
					UpdatePosition();
				}
			}

			base.Tick();
		}
	}

	public class EntityHud : Panel
	{
		public static EntityHud Instance { get; private set; }

		public EntityHud()
		{
			StyleSheet.Load( "/ui/EntityHud.scss" );

			Instance = this;
		}

		public EntityHudAnchor Create( IHudEntity entity )
		{
			var container = AddChild<EntityHudAnchor>();
			container.SetEntity( entity );
			return container;
		}
	}
}
