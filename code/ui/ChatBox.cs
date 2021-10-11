
using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public enum ChatBoxChannel
	{
		All,
		Team
	}

	public class TextEntryContainer : Panel
	{
		public TabTextEntry Input { get; private set; }
		public Label Channel { get; private set; }

		public TextEntryContainer()
		{
			Channel = Add.Label( "[All]", "channel" );
			Input = AddChild<TabTextEntry>( "" );
		}

		public void SetChannel( ChatBoxChannel channel )
		{
			Channel.SetClass( "team", channel == ChatBoxChannel.Team );
			Channel.SetClass( "all", channel == ChatBoxChannel.All );

			if ( channel == ChatBoxChannel.All )
				Channel.Text = "[All]";
			else
				Channel.Text = "[Team]";
		}
	}

	public partial class ChatBox : Panel
	{
		public static ChatBox Current { get; private set; }

		public TextEntryContainer TextEntry { get; private set; }
		public Panel Canvas { get; private set; }

		[ClientCmd( "chat_add", CanBeCalledFromServer = true )]
		public static void AddChatEntry( string name, string message, string avatar = null, string className = null, bool teamOnly = false )
		{
			Current?.AddEntry( name, message, avatar, className, teamOnly );

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
		public static void Say( string message, ChatBoxChannel channel )
		{
			var caller = ConsoleSystem.Caller;
			Assert.NotNull( caller );

			if ( caller.Pawn is not Player player )
				return;

			if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
				return;

			Log.Info( $"{caller}: {message}" );

			if ( channel == ChatBoxChannel.All )
				AddChatEntry( To.Everyone, caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}", player.Team.GetHudClass() );
			else
				AddChatEntry( player.Team.GetTo(), caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}", player.Team.GetHudClass(), true );
		}

		[ClientCmd( "openchat" )]
		public static void OnOpenChat()
		{
			Current?.Open();
		}

		public ChatBoxChannel Channel { get; private set; } = ChatBoxChannel.Team;

		public ChatBox()
		{
			Current = this;

			StyleSheet.Load( "/ui/ChatBox.scss" );

			Canvas = Add.Panel( "chat_canvas" );

			TextEntry = AddChild<TextEntryContainer>();
			TextEntry.Input.AddEventListener( "onsubmit", () => Submit() );
			TextEntry.Input.AddEventListener( "onblur", () => Close() );
			TextEntry.Input.AcceptsFocus = true;
			TextEntry.Input.AllowEmojiReplace = true;
			TextEntry.Input.OnTabPressed += OnTabPressed;
			TextEntry.SetChannel( Channel );
		}

		private void OnTabPressed()
		{
			if ( Channel == ChatBoxChannel.All )
				Channel = ChatBoxChannel.Team;
			else
				Channel = ChatBoxChannel.All;

			TextEntry.SetChannel( Channel );
		}

		public void Open()
		{
			AddClass( "open" );
			TextEntry.Input.Focus();
		}

		public void Close()
		{
			RemoveClass( "open" );
			TextEntry.Input.Blur();
		}

		public void AddEntry( string name, string message, string avatar, string className = null, bool teamOnly = false )
		{
			var entry = Canvas.AddChild<ChatEntry>();
			entry.Message.Text = message;
			entry.NameLabel.Text = teamOnly ? $"[Team] {name}" : name;
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

			var msg = TextEntry.Input.Text.Trim();
			TextEntry.Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			Say( msg, Channel );
		}
	}
}
