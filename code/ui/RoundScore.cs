
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover.UI
{
	public class RoundScoreItem : Panel
	{
		public Label Score { get; set; }

		public RoundScoreItem()
		{
			Score = Add.Label( "0", "score" );
		}
	}

	[StyleSheet( "/ui/RoundScore.scss" )]
	public class RoundScore : Panel
	{
		public Panel Container;
		public RoundScoreItem Blue;  
		public RoundScoreItem Red;  

		public RoundScore()
		{
			Container = Add.Panel( "container" );
			Blue = Container.AddChild<RoundScoreItem>( "blue" );
			Red = Container.AddChild<RoundScoreItem>( "red" );
		}

		public override void Tick()
		{
			var player = Game.LocalPawn;
			if ( player == null ) return;

			if ( HoverGame.Round is PlayRound round )
			{
				Blue.Score.Text = round.BlueScore.ToString();
				Red.Score.Text = round.RedScore.ToString();
			}

			base.Tick();
		}
	}
}
