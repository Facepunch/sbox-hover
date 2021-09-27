using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public static partial class Teams
	{
		public static RedTeam Red { get; private set; }
		public static BlueTeam Blue { get; private set; }
		public static List<BaseTeam> All { get; private set; }

		public static void Initialize()
		{
			All = new();

			Red = new RedTeam();
			AddTeam( Red );

			Blue = new BlueTeam();
			AddTeam( Blue );
		}

		public static void AddTeam( BaseTeam team )
		{
			All.Add( team );
			team.Index = All.Count;
		}

		public static BaseTeam GetByIndex( int index )
		{
			return All[index - 1];
		}

		public static List<Player> GetPlayers<T>( bool isAlive = false ) where T : BaseTeam
		{
			var output = new List<Player>();

			foreach ( var client in Client.All )
			{
				if ( client.Pawn is Player player && player.Team is T )
				{
					if ( !isAlive || player.LifeState == LifeState.Alive )
					{
						output.Add( player );
					}
				}
			}

			return output;
		}

		[Event.Tick]
		private static void OnTick()
		{
			for ( var i = 0; i < All.Count; i++ )
			{
				All[i].OnTick();
			}
		}
	}
}
