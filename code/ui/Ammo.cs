
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	[UseTemplate] 
	public class Ammo : Panel
	{
		public Panel TextContainer { get; set; }
		public Label Weapon { get; set; }
		public Label Inventory { get; set; }

		public Panel Icon { get; set; }

		public Ammo()
		{

			
			
		}

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			var weapon = player.ActiveChild as Weapon;
			var isValid = (weapon != null && !weapon.IsMelee);

			SetClass( "active", isValid );
			SetClass("low", weapon != null && weapon.AmmoClip < 3);

			if ( !isValid ) return;

			Weapon.Text = $"{weapon.AmmoClip}";

			if ( !weapon.UnlimitedAmmo )
			{
				var inv = weapon.AvailableAmmo();
				Inventory.Text = $"/ {inv}";
				Inventory.SetClass( "active", inv >= 0 );
			}
			else
			{
				Inventory.Text = $"/ ∞";
				Inventory.SetClass( "active", true );
			}
		}
	}
}
