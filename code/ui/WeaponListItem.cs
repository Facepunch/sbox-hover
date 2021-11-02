
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
		public Weapon Weapon { get; private set; }
		public Label Ability { get; private set; }
		public Image Icon { get; private set; }
		public Label Name { get; private set; }

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
}
