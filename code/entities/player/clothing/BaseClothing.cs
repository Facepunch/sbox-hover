using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class BaseClothing : ModelEntity
	{
		public Player Wearer => Parent as Player;

		public virtual void Attached() { }

		public virtual void Detatched() { }
	}
}
