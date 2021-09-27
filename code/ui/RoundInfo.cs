
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public class RoundInfo : Panel
	{
		public Panel Container;
		public Label RoundName;
		public Label TimeLeft;
		public Panel Icon;

		public RoundInfo()
		{
			StyleSheet.Load( "/ui/RoundInfo.scss" );

			Container = Add.Panel( "roundContainer" );
			RoundName = Container.Add.Label( "Round", "roundName" );
			Icon = Container.Add.Panel( "icon" );
			TimeLeft = Container.Add.Label( "00:00", "timeLeft" );
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
