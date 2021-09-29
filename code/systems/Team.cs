using Sandbox;
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

		public static string GetName( this Team team )
		{
			return team == Team.Blue ? "Snakes" : "Scorpions";
		}

		public static int GetCount( this Team team )
		{
			return Entity.All.OfType<Player>().Where( e => e.Team == team ).Count();
		}
	}
}
