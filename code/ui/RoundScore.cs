
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public class RoundScoreItem : Panel
	{
		public Panel Icon {get; set;}
		public Label Score {get; set;}

		public RoundScoreItem()
		{
			// Icon = Add.Panel( "icon" );
			// Score = Add.Label( "0", "score" );
		}
	}

	public class RoundScore : Panel
	{
		public Panel Container;
		public RoundScoreItem Blue;  
		public RoundScoreItem Red;  

		public RoundScore()
		{
			StyleSheet.Load( "/ui/RoundScore.scss" );

			Container = Add.Panel( "container" );
			Blue = Container.AddChild<RoundScoreItem>( "blue" );
			Red = Container.AddChild<RoundScoreItem>( "red" );
		}

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			var game = Game.Instance;
			if ( game == null ) return;

			if ( Rounds.Current is PlayRound round )
			{
				Blue.Score.Text = round.BlueScore.ToString();
				Red.Score.Text = round.RedScore.ToString();
			}

			base.Tick();
		}
	}
}
