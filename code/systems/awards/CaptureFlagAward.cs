using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class CaptureFlagAward : Award
	{
		public override string Name => "Captured the Flag";
		public override string Description => "Safely bring the enemy flag to your home base";
		public override int Tokens => 500;
	}
}
