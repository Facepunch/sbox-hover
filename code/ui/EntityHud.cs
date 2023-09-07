using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover.UI
{
	[StyleSheet( "/ui/EntityHud.scss" )]
	public class EntityHudIconList : Panel
	{
		public void SetActive( bool active )
		{
			SetClass( "hidden", !active );
		}
	}

	[StyleSheet( "/ui/EntityHud.scss" )]
	public class EntityHudIcon : Image
	{
		public void SetActive( bool active )
		{
			SetClass( "hidden", !active );
		}
	}

	[StyleSheet( "/ui/EntityHud.scss" )]
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

	[StyleSheet( "/ui/EntityHud.scss" )]
	public class OutpostHud : Panel
	{
		private Label Letter { get; }
		private OutpostVolume Outpost { get; set; }

		public OutpostHud()
		{
			Letter = Add.Label( "", "letter" );
		}

		public void SetOutpost( OutpostVolume outpost )
		{
			Letter.Text = outpost.Letter;
			Outpost = outpost;
		}

		public override void Tick()
		{
			SetClass( Team.Blue.GetHudClass(), Outpost.Team == Team.Blue );
			SetClass( Team.Red.GetHudClass(), Outpost.Team == Team.Red );
			SetClass( Team.None.GetHudClass(), Outpost.Team == Team.None );
			
			base.Tick();
		}
	}

	[StyleSheet( "/ui/EntityHud.scss" )]
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

	[StyleSheet( "/ui/EntityHud.scss" )]
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
			}
		}

		public override void Tick()
		{
			if ( !(Entity as Entity).IsValid() && IsDeleting )
			{
				Delete();
			}

			base.Tick();
		}

		[GameEvent.Client.Frame]
		private void UpdatePosition()
		{
			if ( !(Entity as Entity).IsValid() ) return;

			var position = (Entity.Position + Entity.LocalCenter).ToScreen();

			if ( position.z <= 0f )
			{
				SetClass( "hidden", true );
				return;
			}
			
			if ( Entity.ShouldUpdateHud() )
			{
				Entity.UpdateHudComponents();
				
				position.x *= Screen.Size.x;
				position.y *= Screen.Size.y;
				position.x -= Box.Rect.Width * 0.5f;
				position.y -= Box.Rect.Height * 0.5f;

				Style.Left = Length.Pixels( position.x * ScaleFromScreen );
				Style.Top = Length.Pixels( position.y * ScaleFromScreen );
			}

			SetClass( "hidden", !IsActive );
		}
	}

	public static class EntityHud
	{
		public static EntityHudAnchor Create( IHudEntity entity )
		{
			var container = new EntityHudAnchor();
			container.SetEntity( entity );
			Hud.AddAnchor( container );
			return container;
		}
	}
}
