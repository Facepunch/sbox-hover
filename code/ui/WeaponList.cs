
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Facepunch.Hover
{
	public class WeaponIcon : Panel
	{
		public bool IsUsingAbility { get; set; }
		public bool IsAvailable { get; set; }
		public bool IsPassive { get; set; }
		public bool IsActive { get; set; }
		public bool IsHidden { get; set; }
		public Weapon Weapon { get; private set; }
		public Label Ability { get; private set; }
		public Image Icon { get; private set; }
		public Label Name { get; private set; }

		public WeaponIcon()
		{
			Icon = Add.Image( "", "icon" );
			Name = Add.Label( "", "name" );
			Ability = Add.Label( "", "ability" );
		}

		public void Update( Weapon weapon )
		{
			Weapon = weapon;
			Icon.Texture = Texture.Load( weapon.Config.Icon );
			Name.Text = weapon.Config.Name;

			if ( weapon is Equipment equipment && !string.IsNullOrEmpty( equipment.AbilityBind ) )
			{
				Ability.SetClass( "hidden", false );
				Ability.Text = $"[{Input.GetKeyWithBinding( equipment.AbilityBind )}]";
			}
			else
			{
				Ability.SetClass( "hidden", true );
			}
		}

		public override void Tick()
		{
			SetClass( "using_ability", IsUsingAbility );
			SetClass( "unavailable", !IsAvailable );
			SetClass( "passive", IsPassive );
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

			for ( int i = 0; i < Weapons.Length; i++ )
			{
				var weapon = Weapons[i];
				weapon.IsHidden = true;
			}

			var weapons = player.Children.OfType<Weapon>().ToList();
			weapons.Sort( ( a, b ) => a.Slot.CompareTo( b.Slot ) );

			var currentIndex = 0;

			foreach ( var child in weapons )
			{
				if ( child.Slot < Weapons.Length )
				{
					var weapon = Weapons[currentIndex];

					weapon.Update( child );
					weapon.IsActive = (player.ActiveChild == child);
					weapon.IsHidden = false;
					weapon.IsPassive = child.IsPassive;
					weapon.IsAvailable = child.IsAvailable();

					if ( child is Equipment equipment )
						weapon.IsUsingAbility = equipment.IsUsingAbility;
					else
						weapon.IsUsingAbility = false;

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

		private bool CanSelectWeapon( WeaponIcon weapon )
		{
			if ( !weapon.IsHidden && weapon.Weapon.IsValid()
				&& !weapon.Weapon.IsPassive && weapon.Weapon.IsAvailable() )
			{
				return true;
			}

			return false;
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

			for ( int i = currentIndex; i >= 0; i-- )
			{
				var weapon = Weapons[i];

				if ( !CanSelectWeapon( weapon ) )
					currentIndex--;
				else
					break;
			}

			var firstIndex = GetFirstIndex();
			var lastIndex = GetLastIndex();

			if ( currentIndex < firstIndex )
			{
				currentIndex = lastIndex;
			}

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

			for ( int i = currentIndex; i < Weapons.Length; i++ )
			{
				var weapon = Weapons[i];

				if ( !CanSelectWeapon( weapon ) )
					currentIndex++;
				else
					break;
			}


			var firstIndex = GetFirstIndex();
			var lastIndex = GetLastIndex();

			if ( currentIndex > lastIndex )
				currentIndex = firstIndex;

			SelectWeapon( player, input, currentIndex );
		}

		private int GetLastIndex()
		{
			for ( int i = Weapons.Length - 1; i >= 0; i-- )
			{
				var weapon = Weapons[i];

				if ( CanSelectWeapon( weapon ) )
				{
					return i;
				}
			}

			return 0;
		}

		private int GetFirstIndex()
		{
			for ( int i = 0; i < Weapons.Length; i++ )
			{
				var weapon = Weapons[i];

				if ( CanSelectWeapon( weapon ) )
				{
					return i;
				}
			}

			return 0;
		}

		private void SelectWeapon( Player player, InputBuilder input, int index )
		{
			var weapon = Weapons[index];

			if ( CanSelectWeapon( weapon ) && player.ActiveChild != weapon.Weapon )
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

			WeaponIcon activeWeapon = null;

			for ( int i = 0; i < Weapons.Length; i++ )
			{
				var weapon = Weapons[i];

				if ( weapon.IsActive )
				{
					activeWeapon = weapon;
					break;
				}
			}

			if ( activeWeapon != null && !CanSelectWeapon( activeWeapon ) )
			{
				var firstIndex = GetFirstIndex();
				var firstWeapon = Weapons[firstIndex];

				if ( CanSelectWeapon( firstWeapon ) )
					input.ActiveChild = firstWeapon.Weapon;
				else
					Input.ActiveChild = null;
			}
		}
	}
}
