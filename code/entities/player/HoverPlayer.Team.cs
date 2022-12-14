using Sandbox;

namespace Facepunch.Hover
{
	public partial class HoverPlayer
	{
		[Net, Change] public Team Team { get; private set; }

		public void SetTeam( Team team )
		{
			Team = team;
			Client.SetInt( "team", (int)team );
		}

		protected virtual void OnTeamChanged( Team oldTeam, Team newTeam )
		{
			if ( IsLocalPawn )
			{
				Game.RootPanel.RemoveClass( oldTeam.GetHudClass() );
				Game.RootPanel.AddClass( newTeam.GetHudClass() );
			}
		}
	}
}
