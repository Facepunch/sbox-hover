﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Utility;

namespace Facepunch.ReakSmoke
{
	public partial class SmokePoint : ModelEntity
	{
		private Vector3 OriginalPosition { get; set; }
		private TimeUntil TimeToHide { get; set; }
		private int TickOffset { get; set; }
		private float JiggleSpeed { get; set; }
		private float JiggleRadius { get; set; }
		private bool IsFaded { get; set; }
		private TimeUntil TimeToUnfade { get; set; }
		private Vector3 Center { get; set; }
		private int CreationTick { get; set; }
		private int TargetTick { get; set; }
		private RealSmoke.Type Type { get; set; }

		public void FadeOut( float duration )
		{
			TimeToUnfade = duration;
			IsFaded = true;
		}

		public void Initialize( RealSmoke.Type type, int tick, float volumeSizeSqr, float distanceSqr, Vector3 center, Vector3 position )
		{
			OriginalPosition = position;
			Position = position;
			TickOffset = Game.Random.Int( 0, 256 );
			JiggleSpeed = Game.Random.Float( 0.05f, 0.12f );
			JiggleRadius = Game.Random.Float( 8f, 16f );
			Scale = 1f + Game.Random.Float( -0.15f, 0.1f );
			Center = center;
			Type = type;

			var fraction = Easing.EaseOut( distanceSqr.Remap( 0f, volumeSizeSqr, 0f, 1f ) );

			CreationTick = tick;
			TargetTick = tick + (50 * fraction).CeilToInt() + Game.Random.Int( -5, 5 );
			TimeToHide = 10f + Game.Random.Float( 0.5f ) + (1f - fraction);

			if ( type == RealSmoke.Type.Sphere )
				SetModel( "models/realsmoke/realsmoke.vmdl" );
			else
				SetModel( "models/realsmoke/realsmoke_cube.vmdl" );

			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		}

		public override void Spawn()
		{
			EnableSolidCollisions = false;
			Tags.Add( "realsmoke" );

			base.Spawn();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			EnableDrawing = !IsFaded;

			var position = Center.LerpTo( OriginalPosition, ((float)Time.Tick).LerpInverse( CreationTick, TargetTick ) );
			Position = position + ((MathF.Sin( (Time.Tick + TickOffset) * JiggleSpeed ) * JiggleRadius) + (MathF.Cos( (Time.Tick - TickOffset) * JiggleSpeed ) * JiggleRadius)) * Time.Delta;

			if ( IsFaded && TimeToUnfade )
			{
				IsFaded = false;
			}

			if ( TimeToHide )
			{
				Delete();
			}
		}
	}
}