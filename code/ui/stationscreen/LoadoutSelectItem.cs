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
		public LoadoutSelectList List { get; private set; }

		public void SetList( LoadoutSelectList list )
		{
			List = list;
		}

		public void SetLoadout( BaseLoadout loadout )
		{
			Loadout = loadout;
			Name.Text = loadout.Name;
			Icon.Texture = Texture.Load( FileSystem.Mounted, "ui/icons/player-icon-highlighted.png" );

			if ( Local.Pawn is Player player )
			{
				var roleName = "icon_class_attacker";

				if ( loadout.RoleType == LoadoutRoleType.Defender )
					roleName = "icon_class_defender";
				else if ( loadout.RoleType == LoadoutRoleType.Support )
					roleName = "icon_class_support";

				

				SmallIcon.Texture = Texture.Load( FileSystem.Mounted, $"ui/icons/{roleName}.png" );
			}

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

		protected override void PostTemplateApplied()
		{
			if ( Loadout != null )
			{
				SetLoadout( Loadout );
			}

			base.PostTemplateApplied();
		}

		protected override void OnClick( MousePanelEvent e )
		{
			List?.SetSelectedItem( this );
			Audio.Play( "hover.clickbeep" );
			base.OnClick( e );
		}
	}
}
