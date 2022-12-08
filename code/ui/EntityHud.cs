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

	public class EntityHudAnchor : WorldPanel
	{
		public IHudEntity Entity { get; private set; }
		public float UpOffset { get; set; } = 80f;
		public bool IsActive { get; private set; } = true;

		public EntityHudAnchor()
		{
			StyleSheet.Load( "/ui/EntityHud.scss" );

			//SceneObject.ZBufferMode = ZBufferMode.None;

			PanelBounds = new Rect( -1000, -1000, 2000, 2000 );
		}

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
			}
		}

		public override void Tick()
		{
			if ( (Entity as Entity).IsValid() )
			{
				if ( Entity.ShouldUpdateHud() )
				{
					Entity.UpdateHudComponents();

					var cameraPosition = Camera.Position;
					var transform = Transform;
					var position = (Entity.Position + Entity.LocalCenter) + Vector3.Up * UpOffset;

					transform.Position = position;
					transform.Rotation = Rotation.LookAt( cameraPosition - Position );

					var distanceToCamera = position.Distance( cameraPosition );
					transform.Scale = distanceToCamera.Remap( 0f, 20000f, 10f, 40f );

					Transform = transform;
				}

				SetClass( "hidden", !IsActive );
			}
			else if ( !IsDeleting )
			{
				Delete();
			}

			base.Tick();
		}
	}

	public static class EntityHud
	{
		public static EntityHudAnchor Create( IHudEntity entity )
		{
			var container = new EntityHudAnchor();
			container.SetEntity( entity );
			return container;
		}
	}
}
