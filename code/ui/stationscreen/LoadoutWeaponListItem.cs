using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class LoadoutWeaponListItem : Panel
	{
		public WeaponConfig Config { get; private set; }
		public Label Name { get; private set; }
		public Panel Icon { get; private set; }
		public bool IsActive { get; set; }

		public LoadoutWeaponListItem()
		{
			BindClass( "active", () => IsActive );
		}

		public void SetConfig( WeaponConfig config )
		{
			Name.Text = config.Name;
			Icon.Style.SetBackgroundImage( config.Icon );
			Config = config;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			StationScreen.Instance.SetWeapon( Config );

			CreateEvent( "onpressed" );
			Audio.Play( "hover.clickbeep" );

			base.OnClick( e );
		}
	}
}
