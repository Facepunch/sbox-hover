
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	[UseTemplate] 
	public class RoundInfo : Panel
	{
		public Panel Container { get; set; }
		public Label RoundName { get; set; }
		public Label TimeLeft { get; set; }
		public Panel Icon { get; set; }

		public RoundInfo()
		{
		}

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			var game = Game.Instance;
			if ( game == null ) return;

			var round = Rounds.Current;
			if ( round == null ) return;

			RoundName.Text = round.RoundName;

			if ( round.RoundDuration > 0 )
			{
				TimeLeft.Text = TimeSpan.FromSeconds( round.TimeLeftSeconds ).ToString( @"mm\:ss" );
				Container.SetClass( "roundNameOnly", false );
			}
			else
			{
				Container.SetClass( "roundNameOnly", true );
			}

			base.Tick();
		}
	}
}
