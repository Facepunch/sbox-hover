﻿
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
		public static OutpostList Instance { get; private set; }
		public static List<OutpostVolume> Outposts => new();
		public HashSet<OutpostItem> Items { get; private set; }
		
		public Panel Container { get; private set; }

		public static void AddOutpost( OutpostVolume outpost )
		{
			if ( Outposts.Contains( outpost ) ) return;

			if ( Instance != null )
			{
				Instance.AddItem( outpost );
			}

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

			BindClass( "hidden", ShouldHidePanel );

			Instance = this;
		}

		public void AddItem( OutpostVolume outpost )
		{
			var item = Container.AddChild<OutpostItem>( "outpost" );
			item.SetOutpost( outpost );
			Items.Add( item );
		}

		private bool ShouldHidePanel()
		{
			return (HoverGame.Round is not PlayRound);
		}
	}
}
