
using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.Hover
{
	public class WorldHealthBar : WorldPanel
	{
		public ModelEntity Entity { get; private set; }
		public SimpleIconBar IconBar { get; private set; }
		public float MaximumValue { get; set; }
		public string Attachment { get; set; }
		public new float WorldScale { get; set; } = 1f;
		public bool RotateToFace { get; set; }
		public bool ShowIcon { get; set; } = true;

		public WorldHealthBar()
		{
			StyleSheet.Load( "/ui/WorldHealthBar.scss" );
			IconBar = AddChild<SimpleIconBar>();
		}

		public void SetEntity( ModelEntity entity, string attachment )
		{
			Entity = entity;
			Attachment = attachment;
		}

		public void SetValue( float value )
		{
			var fraction = Length.Fraction( value / MaximumValue );

			if ( IconBar.InnerBar.Style.Width != fraction )
			{
				IconBar.InnerBar.Style.Width = fraction;
				IconBar.InnerBar.Style.Dirty();
				IconBar.InnerBar.SetClass( "hidden", value == 0f );
			}
		}

		public void SetIsLow( bool isLow )
		{
			IconBar.SetClass( "low", isLow );
		}

		public override void Tick()
		{
			if ( IsDeleting ) return;

			if ( !Entity.IsValid() )
			{
				Delete();
				return;
			}

			var attachment = Entity.GetAttachment( Attachment );

			if ( attachment.HasValue )
			{
				Transform = attachment.Value.WithScale( WorldScale );
			}

			if ( RotateToFace )
			{
				var transform = Transform;
				transform.Rotation = Rotation.LookAt( CurrentView.Position - Position );
				Transform = transform;
			}

			IconBar.Icon.SetClass( "hidden", !ShowIcon );

			base.Tick();
		}
	}
}
