using System;
using System.Buffers;

namespace Facepunch.Hover;

public struct VelocityClipPlanes : IDisposable
{
	private Vector3 OrginalVelocity;
	private Vector3 BumpVelocity;
	private Vector3[] Planes;

	public int Max { get; private set; }
	public int Count { get; private set; }

	public VelocityClipPlanes( Vector3 originalVelocity, int max = 5 )
	{
		Max = max;
		OrginalVelocity = originalVelocity;
		BumpVelocity = originalVelocity;
		Planes = ArrayPool<Vector3>.Shared.Rent( max );
		Count = 0;
	}

	public void StartBump( Vector3 velocity )
	{
		BumpVelocity = velocity;
		Count = 0;
	}

	public void Dispose()
	{
		ArrayPool<Vector3>.Shared.Return( Planes );
	}

	public bool TryAdd( Vector3 normal, ref Vector3 velocity, float bounce )
	{
		if ( Count == Max )
		{
			velocity = 0;
			return false;
		}

		Planes[Count++] = normal;

		if ( Count == 1 )
		{
			BumpVelocity = ClipVelocity( BumpVelocity, normal, 1.0f + bounce );
			velocity = BumpVelocity;
			return true;
		}

		velocity = BumpVelocity;

		if ( TryClip( ref velocity ) )
		{
			if ( Count == 2 )
			{
				var dir = Vector3.Cross( Planes[0], Planes[1] );
				velocity = dir.Normal * dir.Dot( velocity );
			}
			else
			{
				velocity = Vector3.Zero;
				return true;
			}
		}

		if ( velocity.Dot( OrginalVelocity ) < 0 )
		{
			velocity = 0;
		}

		return true;
	}

	private bool TryClip( ref Vector3 velocity )
	{
		for ( int i = 0; i < Count; i++ )
		{
			velocity = ClipVelocity( BumpVelocity, Planes[i] );

			if ( MovingTowardsAnyPlane( velocity, i ) )
				return false;
		}

		return true;
	}

	private bool MovingTowardsAnyPlane( Vector3 velocity, int iSkip )
	{
		for ( int j = 0; j < Count; j++ )
		{
			if ( j == iSkip ) continue;
			if ( velocity.Dot( Planes[j] ) < 0 ) return false;
		}

		return true;
	}

	private Vector3 ClipVelocity( Vector3 vel, Vector3 norm, float overbounce = 1.0f )
	{
		var backoff = Vector3.Dot( vel, norm ) * overbounce;
		var o = vel - (norm * backoff);

		var adjust = Vector3.Dot( o, norm );
		if ( adjust >= 1.0f ) return o;

		adjust = MathF.Min( adjust, -1.0f );
		o -= norm * adjust;

		return o;
	}
}
