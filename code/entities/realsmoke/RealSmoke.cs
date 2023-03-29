using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Facepunch.ReakSmoke
{
	public static partial class RealSmoke
	{
		public enum Type
		{
			Sphere,
			Cube
		}

		private static List<SmokePoint> SmokePoints { get; set; } = new();

		public static void Remove( SmokePoint point )
		{
			SmokePoints.Remove( point );
		}

		public static void Create( Type type, Vector3 position, float size, float pointScale = 1f )
		{
			CreateOnClient( To.Everyone, type, Time.Tick, position, size, pointScale );
		}

		public static void Cut( Vector3 start, Vector3 end )
		{
			CutOnClient( To.Everyone, Time.Tick, start, end );
		}

		[ClientRpc]
		public static void CutOnClient( int tick, Vector3 start, Vector3 end )
		{
			var trace = Trace.Ray( start, end )
				.WorldAndEntities()
				.WithAnyTags( "solid" )
				.Size( 4f )
				.Run();

			end = trace.EndPosition;

			foreach ( var s in SmokePoints )
			{
				if ( s.DistanceToLine( start, end ) <= s.Size )
				{
					s.FadeOut();
				}
			}
		}

		[ClientRpc]
		public static void CreateOnClient( Type type, int tick, Vector3 position, float size, float pointScale = 1f )
		{
			var pointRadius = 32f * pointScale;
			var pointSizeWithSpacing = pointRadius * 1.5f;

			if ( type == Type.Cube )
			{
				pointRadius = 48f * pointScale;
				pointSizeWithSpacing = pointRadius;
			}

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
						point.Initialize( type, pointScale, tick, sizeSquared, distanceToCenter, position, positionInSphere );
						SmokePoints.Add( point );
					}
				}
			}
		}

		[Event.Tick.Client]
		private static void ClientTick()
		{
			for ( var i = SmokePoints.Count - 1; i >= 0; i-- )
			{
				SmokePoints[i].Tick();
			}
		}
	}
}
