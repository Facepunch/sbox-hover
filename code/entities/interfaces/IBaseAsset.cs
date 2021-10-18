namespace Facepunch.Hover
{
	public interface IBaseAsset
	{
		public Team Team { get; set; }
		public void SetTeam( Team team );
	}
}
