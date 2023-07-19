using Sandbox;
using Sandbox.UI;

namespace Facepunch.Hover.UI
{
	[StyleSheet( "/ui/WorldHealthBar.scss" )]
	public class WorldHealthBar : WorldPanel
	{
		public ModelEntity Entity { get; private set; }
		public SimpleIconBar IconBar { get; private set; }
		public float MaximumValue { get; set; }
		public string Attachment { get; set; }
		public bool OnlyShowWhenDamaged { get; set; } = false;
		public bool RotateToFace { get; set; }
		public bool ShowIcon { get; set; } = true;

		public WorldHealthBar()
		{
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
			IconBar.InnerBar.Style.Width = fraction;
			IconBar.InnerBar.SetClass( "hidden", value == 0f );

			if ( OnlyShowWhenDamaged )
			{
				SetClass( "hidden", value >= MaximumValue );
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
				Transform = attachment.Value.WithScale( 1.5f );
			}

			if ( RotateToFace )
			{
				var transform = Transform;
				transform.Rotation = Rotation.LookAt( Camera.Position - Position );
				Transform = transform;
			}

			IconBar.Icon.SetClass( "hidden", !ShowIcon );

			base.Tick();
		}
	}
}
