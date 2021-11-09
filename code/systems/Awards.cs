using Gamelib.Network;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public static partial class Awards
	{
		private static Dictionary<string, Award> Lookup { get; set; } = new();

		public static void Add<T>() where T : Award, new()
		{
			var award = new T();
			var type = typeof( T ).Name;

			if ( !Lookup.ContainsKey( type ) )
            {
				Lookup.Add( type, award );
            }
		}

		public static Award Get( string name )
		{
			if ( Lookup.TryGetValue( name, out var award ) )
				return award;
			else
				return default;
		}

		public static Award Get<T>() where T : Award
		{
			var type = typeof( T ).Name;
			return Get( type );
		}
	}
}
