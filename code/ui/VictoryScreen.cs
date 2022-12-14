using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class VictoryScreenAward : Panel
	{
		public Image Icon { get; private set; }
		public Label Count { get; private set; }
		public Award Award { get; private set; }

		public VictoryScreenAward( Award award, int count )
		{
			Icon = Add.Image( "", "icon" );
			Icon.Texture = award.GetShowIcon();

			Count = Add.Label( $"x{count}", "count" );

			Award = award;
		}
	}

	public partial class VictoryScreen : Panel
	{
		public static VictoryScreen Instance { get; private set; }

		[ClientRpc]
		public static void Show( Team winner, float nextGameTime )
		{
			Instance.SetClass( "hidden", false );
			Instance.SetWinner( winner, nextGameTime );
			Instance.AddAwards();
		}

		[ClientRpc]
		public static void Hide()
		{
			Instance.SetClass( "hidden", true );
		}

		public Panel Container { get; private set; }
		public Label NextGameLabel { get; private set; }
		public Panel WinnerContainer { get; private set; }
		public Image WinnerIcon { get; private set; }
		public Label WinnerName { get; private set; }
		public Panel AwardContainer { get; private set; }
		public RealTimeUntil NextGameTime { get; private set; }

		public VictoryScreen()
		{
			StyleSheet.Load( "/ui/VictoryScreen.scss" );

			Container = Add.Panel( "container" );

			WinnerContainer = Container.Add.Panel( "winner" );
			WinnerIcon = WinnerContainer.Add.Image( "", "icon" );
			WinnerName = WinnerContainer.Add.Label( "", "name" );

			NextGameLabel = Container.Add.Label( "", "next_game" );
			AwardContainer = Container.Add.Panel( "awards" );

			SetClass( "hidden", true );

			Instance = this;
		}

		public void SetWinner( Team winner, float nextGameTime )
		{
			if ( winner == Team.Blue )
			{
				WinnerIcon.SetTexture( "ui/icons/blue.png" );
				WinnerName.Text = "Blue Victory";
			}
			else if ( winner == Team.Red )
			{
				WinnerIcon.SetTexture( "ui/icons/red.png" );
				WinnerName.Text = "Red Victory";
			}
			else
			{
				WinnerIcon.SetTexture( "ui/icons/power.png" );
				WinnerName.Text = "Tie";
			}

			Container.SetClass( Team.Red.GetHudClass(), Team.Red == winner );
			Container.SetClass( Team.Blue.GetHudClass(), Team.Blue == winner );
			Container.SetClass( Team.None.GetHudClass(), Team.None == winner );

			NextGameTime = nextGameTime;
		}

		public void AddAwards()
		{
			if ( Game.LocalPawn is not HoverPlayer player )
				return;

			AwardContainer.DeleteChildren( true );

			var awards = new Dictionary<Award, int>();

			foreach ( var award in player.EarnedAwards )
			{
				if ( awards.ContainsKey( award ) )
					awards[award] += 1;
				else
					awards[award] = 1;
			}

			foreach ( var kv in awards )
			{
				var item = new VictoryScreenAward( kv.Key, kv.Value );
				AwardContainer.AddChild( item );
			}

			AwardContainer.SortChildren<VictoryScreenAward>( ( a ) => a.Award.Tokens );
			AwardContainer.SetClass( "hidden", AwardContainer.ChildrenCount == 0 );
		}

		public override void Tick()
		{
			NextGameLabel.Text = $"Next Battle: {NextGameTime.Relative.CeilToInt()}s";

			base.Tick();
		}
	}
}
