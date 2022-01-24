
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Gamelib.UI;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public class BindingLabel : Panel
	{
		public Label ActionLabel { get; private set; }
		public Label HoldLabel { get; private set; }
		public Image Glyph { get; private set; }

		public InputButton Action { get; private set; }

		public override void SetContent( string value )
		{
			ActionLabel.Text = value;

			base.SetContent( value );
		}

		public override void SetProperty( string name, string value )
		{
			if ( name == "hold" )
			{
				HoldLabel.SetClass( "visible", true );
				return;
			}

			if ( name == "action" )
			{
				Action = Enum.Parse<InputButton>( value );
				return;
			}

			base.SetProperty( name, value );
		}

		public override void Tick()
		{
			Glyph.Texture = Input.GetGlyph( Action, InputGlyphSize.Medium );

			base.Tick();
		}
	}
}
