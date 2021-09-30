using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class ReturnFlagAward : Award
	{
		public override string Name => "Returned the Flag";
		public override bool TeamReward => true;
		public override int Tokens => 100;
	}
}
