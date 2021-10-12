
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Facepunch.Hover
{
	public class WeaponIcon : Panel
	{
		public bool IsActive { get; set; }
		public bool IsHidden { get; set; }
		public Weapon Weapon { get; private set; }
		public Image Icon { get; private set; }
		public Label Name { get; private set; }

		public WeaponIcon()
		{
			Icon = Add.Image( "", "icon" );
			Name = Add.Label( "", "name" );
		}

		public void Update( Weapon weapon )
		{
			Weapon = weapon;
			Icon.Texture = weapon.Icon;
			Name.Text = weapon.WeaponName;
		}

		public override void Tick()
		{
			SetClass( "hidden", IsHidden );
			SetClass( "active", IsActive );

			base.Tick();
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
				weapon.IsHidden = true;
				weapon.IsActive = false;
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
				weapon.IsHidden = true;
			}

			var weapons = player.Children.OfType<Weapon>().ToList();
			weapons.Sort( ( a, b ) => a.Slot.CompareTo( b.Slot ) );

			var currentIndex = 0;

			foreach ( var child in weapons )
			{
				if ( child.Slot < 6 )
				{
					var weapon = Weapons[currentIndex];
					weapon.Update( child );
					weapon.IsActive = (player.ActiveChild == child);
					weapon.IsHidden = false;
					currentIndex++;
				}
			}
		}

		private int SlotPressInput( InputBuilder input )
		{
			if ( input.Pressed( InputButton.Slot1 ) ) return 1;
			if ( input.Pressed( InputButton.Slot2 ) ) return 2;
			if ( input.Pressed( InputButton.Slot3 ) ) return 3;
			if ( input.Pressed( InputButton.Slot4 ) ) return 4;
			if ( input.Pressed( InputButton.Slot5 ) ) return 5;
			if ( input.Pressed( InputButton.Slot6 ) ) return 6;

			return -1;
		}

		private void PreviousWeapon( Player player, InputBuilder input )
		{
			var currentIndex = 0;

			for ( int i = 0; i < Weapons.Length; i++ )
			{
				var weapon = Weapons[i];

				if ( weapon.IsActive )
				{
					currentIndex = i;
					break;
				}
			}

			currentIndex--;
			currentIndex = currentIndex.UnsignedMod( 6 );

			SelectWeapon( player, input, currentIndex );
		}

		private void NextWeapon( Player player, InputBuilder input )
		{
			var currentIndex = 0;

			for ( int i = 0; i < Weapons.Length; i++ )
			{
				var weapon = Weapons[i];

				if ( weapon.IsActive )
				{
					currentIndex = i;
					break;
				}
			}

			currentIndex++;
			currentIndex = currentIndex.UnsignedMod( 6 );

			SelectWeapon( player, input, currentIndex );
		}

		private void SelectWeapon( Player player, InputBuilder input, int index )
		{
			var weapon = Weapons[index];

			if ( weapon.Weapon.IsValid() && player.ActiveChild != weapon.Weapon )
			{
				input.ActiveChild = weapon.Weapon;
			}
		}

		[Event.BuildInput]
		private void ProcessClientInput( InputBuilder input )
		{
			if ( Local.Pawn is not Player player )
				return;

			if ( input.MouseWheel == 1 )
			{
				NextWeapon( player, input );
				input.MouseWheel = 0;
			}
			else if ( input.MouseWheel == -1 )
			{
				PreviousWeapon( player, input );
				input.MouseWheel = 0;
			}
			else
			{
				var pressedInput = SlotPressInput( input );

				if ( pressedInput != -1 )
				{
					SelectWeapon( player, input, pressedInput - 1 );
				}
			}
		}
	}
}
