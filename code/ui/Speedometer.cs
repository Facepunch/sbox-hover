
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class Speedometer : Panel
	{
		public Panel Icon;
		public Label Amount;

		public Speedometer()
		{
			Icon = Add.Panel( "icon" );
			Amount = Add.Label( "0", "speed" );
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player )
				return;

			SetClass( "hidden", player.LifeState != LifeState.Alive );

			var velocity = (player.Velocity.Length * 0.0254f).CeilToInt();
			Amount.Text = $"{velocity}m/s";
		}
	}
}
