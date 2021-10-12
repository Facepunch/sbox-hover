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
			return Vector3.Cross( ray.Direction, self - ray.Origin ).Length;
		}

		/*
		public static Vector3 Unproject( this Vector3 self, Matrix inverseViewProjection )
		{
			self.x = (2 * self.x / Screen.Width) - 1f;
			self.y = (2 * self.y / Screen.Height) - 1f;
			self.z = (2 * self.z);

			var mat = inverseViewProjection.Numerics;

			var x1 = self.x * mat.M11 + self.y * mat.M12 + self.z * mat.M13 + mat.M14;
			var y1 = self.x * mat.M21 + self.y * mat.M22 + self.z * mat.M23 + mat.M24;
			var z1 = self.x * mat.M31 + self.y * mat.M32 + self.z * mat.M33 + mat.M34;

			return new Vector3( x1, y1, z1 );
		}
		*/

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
