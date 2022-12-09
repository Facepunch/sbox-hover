
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	[UseTemplate] 
	public class Speedometer : Panel
	{
		public Panel Icon { get; set; }
		public Label Amount { get; set; }

		public Speedometer()
		{

		}

		public override void Tick()
		{
			if ( Local.Pawn is not HoverPlayer player )
				return;

			SetClass( "hidden", player.LifeState != LifeState.Alive );

			var velocity = (player.Velocity.Length * 0.0254f).CeilToInt();
			Amount.Text = $"{velocity}m/s";
		}
	}
}
