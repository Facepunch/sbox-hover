using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class BaseClothing : ModelEntity
	{
		public HoverPlayer Wearer => Parent as HoverPlayer;

		public virtual void Attached() { }

		public virtual void Detatched() { }
	}
}
