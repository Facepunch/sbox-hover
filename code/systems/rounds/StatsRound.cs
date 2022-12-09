using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Hover
{
	public partial class StatsRound : BaseRound
	{
		public override string RoundName => "STATS";
		public override int RoundDuration => 10;

		protected override void OnStart()
		{
			
		}

		protected override void OnFinish()
		{
			if ( Host.IsClient )
			{
				VictoryScreen.Hide();
			}

			base.OnFinish();
		}

		protected override void OnTimeUp()
		{
			if ( Host.IsServer )
			{
				Game.ChangeRound( new PlayRound() );
			}

			base.OnTimeUp();
		}
	}
}
