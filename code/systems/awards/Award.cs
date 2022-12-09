using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public abstract partial class Award
	{
		public virtual Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/blue.png" );
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual bool TeamReward => false;
		public virtual int Tokens => 50;

		public virtual Texture GetShowIcon()
		{
			return Icon;
		}

		public virtual void Show()
		{
			var item = new UI.AwardItem();
			item.Update( Name, Description );
			item.SetIcon( GetShowIcon() );
			item.SetReward( Tokens );
			UI.AwardQueue.Instance.AddItem( item );
		}
	}
}
