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
		public bool IsDisabled { get; private set; }

		public TutorialScreenButton()
		{
			Label = Add.Label( "" );

			BindClass( "disabled", () => IsDisabled );
		}

		public void SetText( string text )
		{
			Label.Text = text;
		}

		public void SetDisabled( bool isDisabled )
		{
			IsDisabled = isDisabled;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			if ( IsDisabled ) return;
			OnClicked?.Invoke();
			base.OnClick( e );
		}
	}

	[UseTemplate]
	public partial class TutorialScreen : Panel
	{
		public static TutorialScreen Instance { get; private set; }

		[ClientVar( "hv_always_show_tutorial" )]
		public static bool AlwaysShowTutorial { get; set; }

		public Panel Container { get; private set; }
		public Panel StepsContainer { get; private set; }
		public Panel ButtonContainer { get; private set; }
		public TutorialScreenButton OkayButton { get; private set; }
		public RealTimeUntil HideTime { get; private set; }

		[ClientRpc]
		public static void Show()
		{
			if ( !AlwaysShowTutorial && Cookie.Get( "tutorial", false ) )
			{
				// Don't show them the screen again.
				return;
			}

			Instance.SetClass( "hidden", false );
			Instance.HideTime = 5f;
		}

		[ClientRpc]
		public static void Hide()
		{
			Instance.SetClass( "hidden", true );
			Cookie.Set( "tutorial", true );
		}

		public TutorialScreen()
		{
			OkayButton.OnClicked = () =>
			{
				Audio.Play( "hover.clickbeep" );
				Hide();
			};
			
			SetClass( "hidden", true );
			Instance = this;
		}

		public override void Tick()
		{
			OkayButton.SetDisabled( !HideTime );

			if ( HideTime )
				OkayButton.SetText( $"Continue" );
			else
				OkayButton.SetText( $"Continue ({ HideTime.Relative.CeilToInt() })" );
		}
	}
}
