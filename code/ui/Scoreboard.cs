
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[UseTemplate]
	public class Scoreboard : Panel
	{
		public Dictionary<Client, ScoreboardEntry> Rows = new();

		public Panel BlueSection { get; set; }
		public Panel RedSection { get; set; }

		public string TimeLeft => GetTimeLeftFormatted();

		public string BlueCaptures => GetFlagCaptures( Team.Blue ).ToString();
		public string RedCaptures => GetFlagCaptures( Team.Red ).ToString();

		public string BlueMembers => Team.Blue.GetCount().ToString();
		public string RedMembers => Team.Red.GetCount().ToString();

		public override void Tick()
		{
			base.Tick();
			
			SetClass( "open", Input.Down( InputButton.Score ) );

			if ( !IsVisible )
				return;

			foreach ( var client in Client.All.Except( Rows.Keys ) )
			{
				var entry = AddClient( client );
				Rows[client] = entry;
			}

			foreach ( var client in Rows.Keys.Except( Client.All ) )
			{
				if ( Rows.TryGetValue( client, out var row ) )
				{
					row?.Delete();
					Rows.Remove( client );
				}
			}

			foreach ( var kv in Rows )
			{
				CheckTeamIndex( kv.Value);
			}
		}

		public void DoSwitchTeam()
		{
			Player.SwitchTeam();
		}

		protected virtual ScoreboardEntry AddClient( Client entry )
		{
			var team = (Team)entry.GetInt( "team" );
			var section = BlueSection;

			if ( team == Team.Red )
            {
				section = RedSection;
            }

			var p = section.AddChild<ScoreboardEntry>();
			p.Client = entry;
			return p;
		}

		private string GetTimeLeftFormatted()
		{
			if ( Rounds.Current is PlayRound round )
				return TimeSpan.FromSeconds( round.TimeLeftSeconds ).ToString( @"mm\:ss" );
			else
				return "00:00";
		}

		private int GetFlagCaptures( Team team )
        {
			if ( Rounds.Current is PlayRound round )
            {
				return team == Team.Blue ? round.BlueScore : round.RedScore;
            }

			return 0;
        }

		private Panel GetTeamSection( Team team )
        {
			return team == Team.Blue ? BlueSection : RedSection;
        }

		private void CheckTeamIndex( ScoreboardEntry entry )
		{
			var team = (Team)entry.Client.GetInt( "team" );
			var section = GetTeamSection( team );

			if ( entry.Parent != section )
            {
				entry.Parent = section;
            }
		}
	}

	public class ScoreboardEntry : Panel
	{
		public Client Client { get; set; }
		public Label PlayerName { get; set; }
		public Label Captures { get; set; }
		public Label Tokens { get; set; }
		public Label Kills { get; set; }
		public Label Deaths { get; set; }
		public Label Ping { get; set; }

		private RealTimeSince TimeSinceUpdate { get; set; }

		public ScoreboardEntry()
		{
			AddClass( "entry" );

			PlayerName = Add.Label( "PlayerName", "name" );
			Tokens = Add.Label( "", "tokens" );
			Captures = Add.Label( "", "captures" );
			Kills = Add.Label( "", "kills" );
			Deaths = Add.Label( "", "deaths" );
			Ping = Add.Label( "", "ping" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			if ( !Client.IsValid() )
				return;

			if ( TimeSinceUpdate < 0.1f )
				return;

			TimeSinceUpdate = 0;
			UpdateData();
		}

		public virtual void UpdateData()
		{
			PlayerName.Text = Client.Name;
			Captures.Text = Client.GetInt( "captures" ).ToString();
			Tokens.Text = $"{Client.GetInt( "tokens" ):C0}";
			Kills.Text = Client.GetInt( "kills" ).ToString();
			Deaths.Text = Client.GetInt( "deaths" ).ToString();
			Ping.Text = Client.Ping.ToString();
			SetClass( "me", Client == Local.Client );
		}

		public virtual void UpdateFrom( Client client )
		{
			Client = client;
			UpdateData();
		}
	}
}
