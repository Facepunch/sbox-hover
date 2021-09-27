using Sandbox;

namespace Facepunch.Hover
{
	public static partial class Audio
	{
		public static void Play( Player player, string sound )
		{
			Play( To.Single( player ), sound );
		}

		public static void Play( Player player, string sound, Vector3 position )
		{
			Play( To.Single( player ), sound, position );
		}

		public static void PlayAll( string sound )
		{
			Play( To.Everyone, sound );
		}

		public static void PlayAll( string sound, Vector3 position )
		{
			Play( To.Everyone, sound, position );
		}

		[ClientRpc]
		public static void Play( string sound )
		{
			Sound.FromScreen( sound );
		}

		[ClientRpc]
		public static void Play( string sound, Vector3 position )
		{
			Sound.FromWorld( sound, position );
		}
	}
}
