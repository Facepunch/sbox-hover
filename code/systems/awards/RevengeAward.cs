using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class RevengeAward : Award
	{
		public override string Name => "Revenge";
		public override string Description => "Kill the player who last killed you";
		public override int Tokens => 100;
	}
}
