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
			if ( team == Team.Blue )
				return "team_blue";
			else if ( team == Team.Red )
				return "team_red";
			else
				return "team_none";
		}

		public static Color GetColor( this Team team )
		{
			if ( team == Team.Blue )
				return Color.Cyan;
			else if ( team == Team.Red )
				return new Color( 1f, 0.38f, 0.27f );
			else
				return new Color( 1f, 1f, 0f );
		}

		public static string GetName( this Team team )
		{
			if ( team == Team.Blue )
				return "Snakes";
			else if ( team == Team.Red )
				return "Scorptions";
			else
				return "Neutral";
		}

		public static To GetTo( this Team team )
		{
			return To.Multiple( team.GetAll().Select( e => e.Client ) );
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
