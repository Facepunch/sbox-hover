using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class LoadoutWeaponItem : Panel
	{
		public WeaponConfig Config { get; private set; }
		public Label Level { get; private set; }
		public Label Name { get; private set; }
		public Image Icon { get; private set; }
		public int Index { get; private set; }

		public override void SetProperty( string name, string value )
		{
			if ( name == "index" )
			{
				Index = int.Parse( value );
				return;
			}

			base.SetProperty( name, value );
		}

		public void SetWeapon( BaseLoadout loadout, WeaponConfig config )
		{
			if ( config == null )
			{
				SetClass( "hidden", true );
				return;
			}

			SetClass( "hidden", false );

			Config = config;

			Name.Text = config.Name;
			Level.Text = loadout.AvailableWeapons[Index].Length.ToString();

			Icon.SetTexture( config.Icon );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			StationScreen.Instance.OpenWeapons( Index );

			Audio.Play( "hover.clickbeep" );
			CreateEvent( "onpressed" );

			base.OnClick( e );
		}
	}
}
