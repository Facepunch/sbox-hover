
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
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
				Audio.Play( "award.earned" );
				var item = Queue.Dequeue();
				item.EndTime = Time.Now + Game.AwardDuration;
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
