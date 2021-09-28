﻿using Sandbox;

namespace Facepunch.Hover
{
	public partial class Player
	{
		[Net, Change] public int TeamIndex { get; set; }
		private BaseTeam _team;

		public BaseTeam Team
		{
			get => _team;

			set
			{
				// A player must be on a valid team.
				if ( value != null && value != _team )
				{
					_team?.Leave( this );
					_team = value;
					_team.Join( this );

					if ( IsServer )
					{
						TeamIndex = _team.Index;

						var client = GetClientOwner();

						// You have to do this for now.
						client.SetScore( "team", TeamIndex );
					}
				}
			}
		}

		private void OnTeamIndexChanged( int newValue )
		{
			Team = Teams.GetByIndex( newValue );
		}
	}
}