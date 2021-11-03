using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public partial class ChatEntry : Panel
	{
		public Label Channel { get; internal set; }
		public Label NameLabel { get; internal set; }
		public Label Message { get; internal set; }
		public Image Avatar { get; internal set; }

		private RealTimeSince TimeSinceBorn { get; set; }

		public ChatEntry()
		{
			TimeSinceBorn = 0f;
			Avatar = Add.Image();

			Channel = Add.Label( "", "channel" );
			Channel.SetClass( "hidden", true );

			NameLabel = Add.Label( "Name", "name" );
			Message = Add.Label( "Message", "message" );
		}

		public void SetChannel( ChatBoxChannel channel )
		{
			if ( channel == ChatBoxChannel.All )
				Channel.Text = "All";
			else if ( channel == ChatBoxChannel.Team )
				Channel.Text = "Team";
			else if ( channel == ChatBoxChannel.Tip )
				Channel.Text = "Tip";

			Channel.SetClass( "hidden", false );
		}

		public override void Tick() 
		{
			base.Tick();

			if ( TimeSinceBorn > 10 ) 
			{ 
				Delete();
			}
		}
	}
}
