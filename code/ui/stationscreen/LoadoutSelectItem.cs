using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public partial class LoadoutSelectItem : Panel
	{
		public Image Icon { get; private set; }
		public Label Level { get; private set; }
		public Image SmallIcon { get; private set; }
		public Label Name { get; private set; }
		public BaseLoadout Loadout { get; private set; }

		public void SetLoadout( BaseLoadout loadout )
		{
			Loadout = loadout;
			Name.Text = loadout.Name;
			Icon.Texture = Texture.Load( "ui/icons/red.png" );
			SmallIcon.Texture = Texture.Load( "ui/icons/icon_energy_white.png" );

			if ( Rand.Int( 1, 2 ) == 2 )
			{
				Level.Text = Rand.Int( 2, 3 ).ToString();
				SetClass( "is-upgrade", true );
			}
			else
			{
				SetClass( "is-upgrade", false );
			}
		}

		protected override void OnClick( MousePanelEvent e )
		{
			if ( Parent is LoadoutSelectList list )
			{
				list.SetSelectedItem( this );
			}

			base.OnClick( e );
		}
	}
}
