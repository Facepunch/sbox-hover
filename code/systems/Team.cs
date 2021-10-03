using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public enum Team
	{
		None,
		Red,
		Blue
	}

	public static class TeamExtensions
	{
		public static string GetHudClass( this Team team )
		{
			return team == Team.Blue ? "team_blue" : "team_red";
		}

		public static Color GetColor( this Team team )
		{
			return team == Team.Blue ? Color.Cyan : new Color( 255, 99, 71 );
		}

		public static string GetName( this Team team )
		{
			return team == Team.Blue ? "Snakes" : "Scorpions";
		}

		public static IEnumerable<Player> GetAll( this Team team )
		{
			return Entity.All.OfType<Player>().Where( e => e.Team == team );
		}

		public static int GetCount( this Team team )
		{
			return Entity.All.OfType<Player>().Where( e => e.Team == team ).Count();
		}
	}
}
