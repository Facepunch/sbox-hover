using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public partial class TutorialScreenButton : Panel
	{
		public Label Label { get; private set; }
		public Action OnClicked { get; set; }

		public TutorialScreenButton()
		{
			Label = Add.Label( "", "label" );
		}

		public void SetText( string text )
		{
			Label.Text = text;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			OnClicked?.Invoke();
			base.OnClick( e );
		}
	}

	public partial class TutorialScreen : Panel
	{
		public static TutorialScreen Instance { get; private set; }

		public Panel Container { get; private set; }
		public TutorialScreenButton OkayButton { get; private set; }

		[ClientRpc]
		public static void Show()
		{
			Instance.SetClass( "hidden", false );
		}

		[ClientRpc]
		public static void Hide()
		{
			Instance.SetClass( "hidden", true );
		}

		public TutorialScreen()
		{
			StyleSheet.Load( "/ui/TutorialScreen.scss" );

			Container = Add.Panel( "container" );
			Container.Add.Label( "How to Play", "title" );
			Container.Add.Label( GetHelpText(), "help" );
			OkayButton = AddChild<TutorialScreenButton>( "button" );
			OkayButton.SetText( "Okay" );
			OkayButton.OnClicked = () =>
			{
				Audio.Play( "hover.clickbeep" );
				Hide();
			};

			SetClass( "hidden", true );

			Instance = this;
		}

		private string GetHelpText()
		{
			return $@"Hold [{Input.GetKeyWithBinding( "iv_jump" )}] to Ski. When you are skiing you maintain your velocity as long as you are not going uphill. You should ski down slopes to gain velocity, and Jetpack by holding [{Input.GetKeyWithBinding( "iv_attack2" )}] to travel over hills without losing velocity while you ski. Keep this up, and you can gain some serious speed! Be careful of your Energy bar when using your Jetpack, you don't want to run out of Energy too high up. Capture the enemy flag, or defend your own, and lead your team to victory.";
		}
	}
}
