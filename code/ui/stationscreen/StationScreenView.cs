using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class StationScreenView : Panel
	{
		public bool IsDefault { get; private set; }
		public string Name { get; private set; }

		public override void SetProperty( string name, string value )
		{
			if ( name == "default" )
			{
				IsDefault = true;
				return;
			}

			if ( name == "name" )
			{
				Name = value;
				return;
			}

			base.SetProperty( name, value );
		}

		public void SetActive( bool isActive )
		{
			SetClass( "hidden", !isActive );
		}

		public StationScreenView()
		{
			SetClass( "hidden", true );
		}
	}
}
