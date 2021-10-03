using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public abstract partial class Award
	{
		public virtual Texture Icon => Texture.Load( "ui/icons/icon-death.png" );
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual bool TeamReward => false;
		public virtual int Tokens => 50;

		public virtual void Show()
		{
			var item = new AwardItem();
			item.Update( Name, Description );
			item.SetIcon( Icon );
			item.SetReward( Tokens );
			AwardQueue.Instance.AddItem( item );
		}
	}
}
