
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover.UI
{
	public class OutpostItem : Panel
	{
		public Label Name { get; private set; }
		public Panel Bar { get; private set; }
		public OutpostVolume Outpost { get; private set; }

		public OutpostItem()
		{
			Bar = Add.Panel( "bar" );

			var content = Add.Panel( "content" );
			var circle = content.Add.Panel( "circle" );

			Name = circle.Add.Label( "", "name" );
		}

		public void SetOutpost( OutpostVolume outpost )
		{
			Name.Text = outpost.Letter;
			Outpost = outpost;
		}

		public override void Tick()
		{
			SetClass( Team.Blue.GetHudClass(), Outpost.Team == Team.Blue );
			SetClass( Team.Red.GetHudClass(), Outpost.Team == Team.Red );
			SetClass( Team.None.GetHudClass(), Outpost.Team == Team.None );

			if ( Outpost.CaptureProgress > 0f )
			{
				Bar.SetClass( "hidden", false );
				Bar.Style.Height = Length.Fraction( Outpost.CaptureProgress );
			}
			else
			{
				Bar.SetClass( "hidden", true );
			}

			base.Tick();
		}
	}

	[StyleSheet( "/ui/OutpostList.scss" )]
	public class OutpostList : Panel
	{
		private static OutpostList Instance { get; set; }
		private static List<OutpostVolume> Outposts { get; } = new();
		private HashSet<OutpostItem> Items { get; }
		private Panel Container { get; }

		public static void RemoveOutpost( OutpostVolume outpost )
		{
			if ( !Outposts.Contains( outpost ) ) return;

			Instance?.RemoveItem( outpost );
			Outposts.Remove( outpost );
		}

		public static void AddOutpost( OutpostVolume outpost )
		{
			if ( Outposts.Contains( outpost ) )
				return;

			Instance?.AddItem( outpost );
			Outposts.Add( outpost );
		}

		public OutpostList()
		{
			Container = Add.Panel( "container" );
			Items = new();

			foreach ( var outpost in Entity.All.OfType<OutpostVolume>() )
			{
				Outposts.Add( outpost );
				AddItem( outpost );
			}

			Instance = this;
		}

		public void RemoveItem( OutpostVolume outpost )
		{
			foreach ( var item in Items.Where( item => item.Outpost == outpost ) )
			{
				Items.Remove( item );
				item.Delete( true );
				return;
			}
		}

		public void AddItem( OutpostVolume outpost )
		{
			var item = Container.AddChild<OutpostItem>( "outpost" );
			item.SetOutpost( outpost );
			Items.Add( item );
		}
	}
}
