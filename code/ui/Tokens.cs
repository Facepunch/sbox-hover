
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	[UseTemplate]
	public class Tokens : Panel
	{
		public Panel Icon { get; set; }
		public Label Amount { get; set; }

		public Tokens()
		{
			
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player )
				return;

			SetClass( "hidden", player.LifeState != LifeState.Alive );

			Amount.Text = $"{player.Tokens:C0}";
		}
	}
}
