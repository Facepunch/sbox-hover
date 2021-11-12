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

	[UseTemplate]
	public partial class TutorialScreen : Panel
	{
		public static TutorialScreen Instance { get; private set; }

		public Panel Container { get; private set; }
		public Panel StepsContainer { get; private set; }
		public Panel ButtonContainer { get; private set; }
		public Label Title { get; private set; }
		public Label StepOne { get; private set; }
		public Label StepTwo { get; private set; }
		public Label StepThree { get; private set; }
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
			Title.Text = "How to Play";
			StepOne.Text = StepOneText();
			StepTwo.Text = StepTwoText();
			StepThree.Text = StepThreeText();
		}

		private string StepOneText() {
			return $@"Hold [{Input.GetKeyWithBinding( "iv_jump" )}] to Ski. When you are skiing you maintain your velocity as long as you are not going uphill.";
		}

		private string StepTwoText() {
			return $@"You should ski down slopes to gain velocity, and Jetpack by holding [{Input.GetKeyWithBinding( "iv_attack2" )}] to travel over hills without losing velocity while you ski.";
		}
		private string StepThreeText() {
			return "Capture the enemy flag, or defend your own, and lead your team to victory.";
		}
	}
}
