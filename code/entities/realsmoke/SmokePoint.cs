using System;
using Sandbox;
using Sandbox.Utility;

namespace Facepunch.ReakSmoke
{
	public partial class SmokePoint
	{
		public RealSmoke.Type Type { get; private set; }
		public float Size => Type == RealSmoke.Type.Cube ? 48f * Scale : 64f * Scale;
		public float Scale { get; private set; }

		private Vector3 OriginalPosition { get; set; }
		private TimeUntil TimeToHide { get; set; }
		private int TickOffset { get; set; }
		private float JiggleSpeed { get; set; }
		private float JiggleRadius { get; set; }
		private bool IsFaded { get; set; }
		private TimeUntil TimeToUnfade { get; set; }
		private Vector3 Center { get; set; }
		private float FadeOutTime { get; set; }
		private int CreationTick { get; set; }
		private int TargetTick { get; set; }
		private SceneObject SceneObject { get; set; }

		public void FadeOut()
		{
			if ( IsFaded ) return;
			TimeToUnfade = FadeOutTime;
			IsFaded = true;
		}

		public float DistanceToLine( Vector3 start, Vector3 end )
		{
			var p = SceneObject.Position;
			var v = end - start;
			var w = p - start;

			var c1 = Vector3.Dot( w, v );
			if ( c1 <= 0 )
			{
				return p.Distance( start );
			}

			var c2 = Vector3.Dot( v, v );
			if ( c2 <= c1 )
			{
				return p.Distance( end );
			}

			var b = c1 / c2;
			var pb = start + b * v;

			return p.Distance( pb );
		}

		public void Initialize( RealSmoke.Type type, float scale, int tick, float volumeSizeSqr, float distanceSqr, Vector3 center, Vector3 position )
		{
			var model = "models/realsmoke/realsmoke.vmdl";

			if ( type == RealSmoke.Type.Cube )
				model = "models/realsmoke/realsmoke_cube.vmdl";

			var transform = new Transform
			{
				Position = position,
				Rotation = Rotation.Identity,
				Scale = scale + Game.Random.Float( -0.15f, 0.1f )
			};

			SceneObject = new SceneObject( Game.SceneWorld, model );
			SceneObject.Transform = transform;

			OriginalPosition = position;
			FadeOutTime = Game.Random.Float( 1.2f, 1.8f );
			TickOffset = Game.Random.Int( 0, 256 );
			JiggleSpeed = Game.Random.Float( 0.05f, 0.12f );
			JiggleRadius = Game.Random.Float( 8f, 16f );
			Center = center;
			Scale = scale;
			Type = type;

			var fraction = Easing.EaseOut( distanceSqr.Remap( 0f, volumeSizeSqr, 0f, 1f ) );

			CreationTick = tick;
			TargetTick = tick + (50 * fraction).CeilToInt() + Game.Random.Int( -5, 5 );
			TimeToHide = 10f + Game.Random.Float( 0.5f ) + (1f - fraction);
		}

		public void Tick()
		{
			SceneObject.RenderingEnabled = !IsFaded;

			var position = Center.LerpTo( OriginalPosition, ((float)Time.Tick).LerpInverse( CreationTick, TargetTick ) );
			SceneObject.Position = position + ((MathF.Sin( (Time.Tick + TickOffset) * JiggleSpeed ) * JiggleRadius) + (MathF.Cos( (Time.Tick - TickOffset) * JiggleSpeed ) * JiggleRadius)) * Time.Delta;

			if ( IsFaded && TimeToUnfade )
			{
				IsFaded = false;
			}

			if ( TimeToHide )
			{
				RealSmoke.Remove( this );
				SceneObject.Delete();
			}
		}
	}
}
