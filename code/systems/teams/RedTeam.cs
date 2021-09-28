using Sandbox;

namespace Facepunch.Hover
{
	public class RedTeam : SimpleTeam
	{
		public override string HudClassName => "team_red";
		public override TeamType Type => TeamType.Red;
		public override string Name => "Red Team";
	}
}
