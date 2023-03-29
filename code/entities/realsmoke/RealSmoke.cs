using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Facepunch.ReakSmoke
{
	public static partial class RealSmoke
	{
		public static void Create( Vector3 position, float size )
		{
			CreateOnClient( To.Everyone, Time.Tick, position, size );
		}

		public static void Cut( Vector3 start, Vector3 end )
		{
			CutOnClient( To.Everyone, Time.Tick, start, end );
		}

		[ClientRpc]
		public static void CutOnClient( int tick, Vector3 start, Vector3 end )
		{
			Game.SetRandomSeed( tick );

			var results = Trace.Ray( start, end )
				.WorldAndEntities()
				.WithAnyTags( "solid", "realsmoke" )
				.IncludeClientside()
				.Size( 4f )
				.RunAll();

			if ( results is null )
				return;

			foreach ( var r in results )
			{
				if ( r.Entity is not SmokePoint s )
					return;

				s.FadeOut( Game.Random.Float( 1.2f, 1.8f ) );
			}
		}

		[ClientRpc]
		public static void CreateOnClient( int tick, Vector3 position, float size )
		{
			var pointRadius = 32f;
			var pointSizeWithSpacing = pointRadius * 1.5f;
			var numPointsPerAxis = (size / pointSizeWithSpacing).CeilToInt();
			var halfPointsPerAxis = numPointsPerAxis / 2;
			var sizeSquared = size * size;
			var squaredRadius = sizeSquared * 0.25f;

			Game.SetRandomSeed( tick );

			for ( var x = -halfPointsPerAxis; x <= halfPointsPerAxis; x++ )
			{
				for ( var y = -halfPointsPerAxis; y <= halfPointsPerAxis; y++ )
				{
					for ( var z = -halfPointsPerAxis; z <= halfPointsPerAxis; z++ )
					{
						var offset = new Vector3( x, y, z ) * pointSizeWithSpacing;
						var positionInSphere = position + offset;
						var distanceToCenter = position.DistanceSquared( positionInSphere );

						if ( distanceToCenter > squaredRadius )
							continue;

						var trace = Trace.Ray( position, positionInSphere )
							.WorldAndEntities()
							.WithAnyTags( "solid" )
							.Run();

						if ( trace.Fraction < 0.8f )
							continue;

						var point = new SmokePoint();
						point.Initialize( positionInSphere );
					}
				}
			}
		}
	}
}
