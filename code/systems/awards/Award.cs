using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public abstract partial class Award
	{
		public virtual Texture Icon => Texture.Load( "ui/icons/icon-death.png" );
		public virtual string Name => "";
		public virtual bool TeamReward => false;
		public virtual int Tokens => 50;
	}
}
