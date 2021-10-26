using Sandbox;
using System;

namespace Gamelib.Tweens
{
	public class Tween
	{
		public delegate void OnCompleteHandler( Tween tween );
		public delegate void OnUpdateHandler( Tween tween, float value );
		public delegate float EasingHandler( float progress );

		protected OnCompleteHandler OnComplete;
		protected OnUpdateHandler OnUpdate;
		protected Func<float, float> Easing;

		public float Progress { get; protected set; }
		public bool IsPingPong { get; protected set; }
		public bool IsLooping { get; protected set; }
		public float Duration { get; protected set; }
		public bool IsPlaying { get; protected set; }
		public int RemainingRuns { get; protected set; }
		public bool IsReversing { get; protected set; }


		public Tween()
		{
			RemainingRuns = 1;
			IsPingPong = false;
			Duration = 0f;
			IsLooping = false;
			IsPlaying = false;
			Easing = null;
		}

		public void Remove()
		{
			if ( IsPlaying )
			{
				Event.Unregister( this );
				IsPlaying = false;
			}
		}

		public Tween SetOnComplete( OnCompleteHandler handler )
		{
			OnComplete = handler;
			return this;
		}

		public Tween SetOnUpdate( OnUpdateHandler handler )
		{
			OnUpdate = handler;
			return this;
		}

		public Tween SetEasing( Func<float, float> handler )
		{
			Easing = handler;
			return this;
		}

		public Tween SetDuration( float duration )
		{
			Duration = duration;
			return this;
		}

		public Tween SetRunCount( int value )
		{
			RemainingRuns = value;
			IsLooping = (value == -1);

			return this;
		}

		public Tween SetPingPong( bool value )
		{
			IsPingPong = value;
			return this;
		}

		public Tween Reset()
		{
			if ( IsPlaying )
			{
				Progress = 0f;
			}

			return this;
		}

		public Tween Start()
		{
			if ( !IsPlaying )
			{
				Event.Register( this );
				IsPlaying = true;
			}

			return this;
		}

		[Event.Tick]
		private void Update()
		{
			if ( Progress < 1f )
			{
				Progress = Math.Clamp( Progress + Time.Delta / Duration, 0f, 1f );

				var value = Easing?.Invoke( Progress ) ?? Progress;

				if ( IsReversing )
				{
					value = 1f - value;
				}

				OnUpdate?.Invoke( this, value );
				return;
			}

			OnUpdate?.Invoke( this, IsReversing ? 0f : 1f );
			OnComplete?.Invoke( this );

			if ( IsPingPong )
			{
				IsReversing = !IsReversing;
			}

			if ( RemainingRuns > 0 )
			{
				RemainingRuns--;

				if ( RemainingRuns == 0 )
					Remove();
				else
					Reset();
			}
			else
			{
				Reset();
			}
		}
	}
}
