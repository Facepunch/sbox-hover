using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class StationScreenButton : Panel
	{
		public Label Label { get; private set; }
		public bool IsDisabled { get; private set; }

		public override void SetProperty( string name, string value )
		{
			if ( name == "text" )
			{
				Label.Text = value;
				return;
			}

			base.SetProperty( name, value );
		}

		public void SetDisabled( bool isDisabled )
		{
			IsDisabled = isDisabled;
		}

		public override void SetContent( string value )
		{
			Label.Text = value;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			if ( !IsDisabled )
			{
				CreateEvent( "onpressed" );
				Audio.Play( "hover.clickbeep" );
			}

			base.OnClick( e );
		}

		public StationScreenButton()
		{
			BindClass( "is-disabled", () => IsDisabled );
		}
	}
}
