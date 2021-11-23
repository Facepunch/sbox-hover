using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.Hover
{
	[UseTemplate]
	public class AwardItem : Panel
	{
		public float EndTime { get; set; }
		public Label Reward { get; set; }
		public Label Title { get; set; }
		public Label Text { get; set; }
		public Image Icon { get; set; }

		public void Update( string title, string text )
		{
			Title.Text = title;
			Text.Text = text;
		}

		public void SetIcon( Texture texture )
		{
			Icon.Texture = texture;
		}

		public void SetReward( int amount )
		{
			Reward.Text = $"{amount:C0}";
		}

		public override void Tick()
		{
			if ( !IsDeleting && Time.Now >= EndTime )
			{
				Delete();
			}
		}
	}
}
