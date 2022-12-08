using Sandbox;
using System;

namespace Gamelib.Extensions
{
	public static class Vector3Extension
	{
		public static string ToCSV( this Vector3 self )
		{
			return (self.x + "," + self.y + "," + self.z);
		}

		public static Vector3 ApplyMatrix( this Vector3 self, Matrix matrix )
		{
			return matrix.Transform( self );
		}

		public static Vector3 InvertXY( this Vector3 self )
		{
			return new Vector3( self.y, self.x, self.z );
		}

		public static float DistanceToLine( this Vector3 self, Vector3 start, Vector3 end, out Vector3 intersection )
		{
			var v = end - start;
			var w = self - start;

			var c1 = Vector3.Dot( w, v );
			if ( c1 <= 0 )
			{
				intersection = start;
				return self.Distance( start );
			}

			var c2 = Vector3.Dot( v, v );
			if ( c2 <= c1 )
			{
				intersection = end;
				return self.Distance( end );
			}

			var b = c1 / c2;
			var pb = start + b * v;

			intersection = pb;
			return self.Distance( pb );
		}

		public static float DistanceToRay( this Vector3 self, Ray ray )
		{
			return Vector3.Cross( ray.Forward, self - ray.Position ).Length;
		}

		public static Vector3 Project( this Vector3 self, Matrix world, Matrix projection )
		{
			return self.ApplyMatrix( world.Inverted ).ApplyMatrix( projection );
		}

		public static Vector3 Unproject( this Vector3 self, Matrix world, Matrix projection )
		{
			return self.ApplyMatrix( projection.Inverted ).ApplyMatrix( world );
		}
	}
}
