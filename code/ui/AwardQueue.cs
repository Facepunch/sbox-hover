
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class AwardItemReward : Panel
	{
		public Label Amount { get; set; }
		public Image Icon { get; set; }

		public AwardItemReward()
		{
			Icon = Add.Image( "ui/icons/tokens.png", "icon" );
			Amount = Add.Label( "0", "amount" );
		}
	}

	public class AwardItem : Panel
	{
		public AwardItemReward Reward { get; set; }
		public Panel Header { get; set; }
		public float EndTime { get; set; }
		public Label Title { get; set; }
		public Label Text { get; set; }
		public Image Icon { get; set; }

		public AwardItem()
		{
			Header = Add.Panel( "header" );
			Icon = Header.Add.Image( "", "icon" );
			Title = Header.Add.Label( "", "title" );
			Text = Add.Label( "", "text" );
			Reward = AddChild<AwardItemReward>( "reward" );
		}

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
			Reward.Amount.Text = amount.ToString();
		}

		public override void Tick()
		{
			if ( !IsDeleting && Time.Now >= EndTime )
			{
				Delete();
			}
		}
	}

	public class AwardQueue : Panel
	{
		public static AwardQueue Instance { get; private set; }

		public Queue<AwardItem> Queue { get; private set; }

		public AwardQueue()
		{
			StyleSheet.Load( "/ui/AwardQueue.scss" );
			Instance = this;
			Queue = new();
		}

		public void AddItem( AwardItem item )
		{
			Queue.Enqueue( item );
		}

		public void Next()
		{
			if ( Queue.Count > 0 )
			{
				var item = Queue.Dequeue();
				item.EndTime = Time.Now + 3f;
				AddChild( item );
			}
		}

		public override void Tick()
		{
			if ( Queue.Count > 0 && ChildrenCount == 0 )
			{
				Next();
			}

			base.Tick();
		}
	}
}
