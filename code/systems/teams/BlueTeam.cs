using Sandbox;

namespace Facepunch.Hover
{
    public class BlueTeam : SimpleTeam
	{
		public override string HudClassName => "team_blue";
		public override TeamType Type => TeamType.Blue;
		public override string Name => "Blue Team";
	}
}
