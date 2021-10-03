
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class Tokens : Panel
	{
		public Panel Icon;
		public Label Amount;

		public Tokens()
		{
			Icon = Add.Panel( "icon" );
			Amount = Add.Label( "0", "amount" );
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player )
				return;

			SetClass( "hidden", player.LifeState != LifeState.Alive );

			Amount.Text = player.Tokens.ToString();
		}
	}
}
