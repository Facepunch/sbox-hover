
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Facepunch.Hover
{
	[UseTemplate]
	public class WeaponListItem : Panel
	{
		public bool IsUsingAbility { get; set; }
		public bool IsAvailable { get; set; }
		public bool IsPassive { get; set; }
		public bool IsActive { get; set; }
		public bool IsHidden { get; set; }
		public string KeyBind { get; set; }
		public Weapon Weapon { get; private set; }
		public Label Slot { get; private set; }
		public Image Icon { get; private set; }
		public Label Name { get; private set; }

		public void Update( Weapon weapon )
		{
			Weapon = weapon;
			Icon.Texture = Texture.Load( weapon.Config.Icon );
			Name.Text = weapon.Config.Name;

			if ( !string.IsNullOrEmpty( KeyBind ) )
            {
				Slot.SetClass( "hidden", false );
				Slot.Text = Input.GetKeyWithBinding( KeyBind ).ToUpper();
            }
			else
            {
				Slot.SetClass( "hidden", true );
				Slot.Text = string.Empty;
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
}
