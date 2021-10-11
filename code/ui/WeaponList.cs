
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Facepunch.Hover
{
	public class WeaponIcon : Panel
	{
		public Weapon Weapon { get; private set; }
		public Image Icon { get; private set; }
		public Label Name { get; private set; }

		public WeaponIcon()
		{
			Icon = Add.Image( "", "icon" );
			Name = Add.Label( "", "name" );
		}

		public void SetActive( bool isActive )
		{
			SetClass( "active", isActive );
		}

		public void SetHidden( bool isHidden )
		{
			SetClass( "hidden", isHidden );
		}

		public void Update( Weapon weapon )
		{
			Weapon = weapon;
			Icon.Texture = weapon.Icon;
			Name.Text = weapon.WeaponName;
		}
	}

	public class WeaponList : Panel
	{
		public WeaponIcon[] Weapons { get; set; } = new WeaponIcon[6];

		public WeaponList()
		{
			StyleSheet.Load( "/ui/WeaponList.scss" );

			for ( int i = 0; i < 6; i++ )
			{
				var weapon = AddChild<WeaponIcon>( "weapon" );
				weapon.SetHidden( true );
				Weapons[i] = weapon;
			}
		}

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is not Player player )
				return;

			for ( int i = 0; i < 6; i++ )
			{
				var weapon = Weapons[i];
				weapon.SetHidden( true );
			}

			var weapons = player.Children.OfType<Weapon>();

			foreach ( var child in weapons )
			{
				if ( child.Slot < 6 )
				{
					var weapon = Weapons[child.Slot];
					weapon.Update( child );
					weapon.SetHidden( false );
					weapon.SetActive( player.ActiveChild == child );
				}
			}
		}

		private int SlotPressInput( InputBuilder input )
		{
			if ( input.Pressed( InputButton.Slot0 ) ) return 0;
			if ( input.Pressed( InputButton.Slot1 ) ) return 1;
			if ( input.Pressed( InputButton.Slot2 ) ) return 2;
			if ( input.Pressed( InputButton.Slot3 ) ) return 3;
			if ( input.Pressed( InputButton.Slot4 ) ) return 4;
			if ( input.Pressed( InputButton.Slot5 ) ) return 5;

			return -1;
		}

		[Event.BuildInput]
		private void ProcessClientInput( InputBuilder input )
		{
			if ( Local.Pawn is not Player player )
				return;

			var selectedIndex = -1;
			var weapons = player.Children.OfType<Weapon>().ToList();

			foreach ( var child in weapons )
			{
				if ( child.Slot >= 6 ) continue;

				if ( selectedIndex == -1 || player.ActiveChild == child )
				{
					selectedIndex = child.Slot;
				}
			}

			var pressedInput = SlotPressInput( input );

			if ( pressedInput != -1 )
			{
				selectedIndex = pressedInput;
			}

			selectedIndex += input.MouseWheel;
			selectedIndex = selectedIndex.UnsignedMod( 6 );

			var weapon = Weapons[selectedIndex];

			if ( weapon.Weapon.IsValid() && player.ActiveChild != weapon.Weapon )
			{
				input.ActiveChild = weapon.Weapon;
				//Audio.Play( "weapon.change" );
			}

			input.MouseWheel = 0;
		}
	}
}
