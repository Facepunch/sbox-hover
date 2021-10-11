
using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public partial class ChatBox : Panel
	{
		public static ChatBox Current { get; private set; }

		public TextEntry Input { get; private set; }
		public Panel Canvas { get; private set; }

		[ClientCmd( "chat_add", CanBeCalledFromServer = true )]
		public static void AddChatEntry( string name, string message, string avatar = null, string className = null )
		{
			Current?.AddEntry( name, message, avatar, className );

			if ( !Global.IsListenServer )
			{
				Log.Info( $"{name}: {message}" );
			}
		}

		[ClientCmd( "chat_addinfo", CanBeCalledFromServer = true )]
		public static void AddInformation( string message, string avatar = null )
		{
			Current?.AddEntry( null, message, avatar, "info" );
		}

		[ServerCmd( "say" )]
		public static void Say( string message )
		{
			var caller = ConsoleSystem.Caller;
			Assert.NotNull( caller );

			if ( caller.Pawn is not Player player )
				return;

			if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
				return;

			Log.Info( $"{caller}: {message}" );
			AddChatEntry( To.Everyone, caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}", player.Team.GetHudClass() );
		}

		[ClientCmd( "openchat" )]
		public static void OnOpenChat()
		{
			Current?.Open();
		}

		public ChatBox()
		{
			Current = this;

			StyleSheet.Load( "/ui/ChatBox.scss" );

			Canvas = Add.Panel( "chat_canvas" );

			Input = Add.TextEntry( "" );
			Input.AddEventListener( "onsubmit", () => Submit() );
			Input.AddEventListener( "onblur", () => Close() );
			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;
		}

		public void Open()
		{
			AddClass( "open" );
			Input.Focus();
		}

		public void Close()
		{
			RemoveClass( "open" );
			Input.Blur();
		}

		public void AddEntry( string name, string message, string avatar, string className = null )
		{
			var entry = Canvas.AddChild<ChatEntry>();
			entry.Message.Text = message;
			entry.NameLabel.Text = name;
			entry.Avatar.SetTexture( avatar );

			if ( !string.IsNullOrEmpty( className ) )
			{
				entry.AddClass( className );
			}

			entry.SetClass( "noname", string.IsNullOrEmpty( name ) );
			entry.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );
		}

		private void Submit()
		{
			Close();

			var msg = Input.Text.Trim();
			Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			Say( msg );
		}
	}
}
