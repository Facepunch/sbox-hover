using Sandbox;

namespace Facepunch.Hover
{
	public partial class Player
	{
		[Net] public Team Team { get; private set; }

		public void SetTeam( Team team )
		{
			Team = team;

			var client = GetClientOwner();
			client.SetScore( "team", (int)team );
		}
	}
}
