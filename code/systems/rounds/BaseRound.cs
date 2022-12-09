using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public abstract partial class BaseRound : BaseNetworkable
	{
		public virtual int RoundDuration => 0;
		public virtual string RoundName => "";
		public virtual bool ShowTimeLeft => false;
		public virtual bool ShowRoundInfo => false;
		public virtual bool CanCaptureFlags => false;
		public virtual bool CanCaptureOutposts => false;

		public List<HoverPlayer> Players = new();
		public RealTimeUntil NextSecondTime { get; private set; }
		public float RoundEndTime { get; set; }

		public float TimeLeft
		{
			get
			{
				return RoundEndTime - Time.Now;
			}
		}

		[Net] public int TimeLeftSeconds { get; set; }

		public void Start()
		{
			if ( Host.IsServer && RoundDuration > 0 )
				RoundEndTime = Time.Now + RoundDuration;

			Event.Register( this );
			
			OnStart();
		}

		public void Finish()
		{
			if ( Host.IsServer )
			{
				RoundEndTime = 0f;
				Players.Clear();
			}

			Event.Unregister( this );

			OnFinish();
		}

		public void AddPlayer( HoverPlayer player )
		{
			Host.AssertServer();

			if ( !Players.Contains(player) )
				Players.Add( player );
		}

		public virtual void OnPlayerKilled( HoverPlayer player, Entity attacker, DamageInfo damageInfo ) { }

		public virtual void OnPlayerSpawn( HoverPlayer player ) { }

		public virtual void OnPlayerJoin( HoverPlayer player ) { }

		public virtual void OnPlayerLeave( HoverPlayer player )
		{
			Players.Remove( player );
		}

		public virtual void OnTick()
		{
			if ( NextSecondTime )
			{
				OnSecond();
				NextSecondTime = 1f;
			}
		}

		public virtual void OnSecond()
		{
			if ( Host.IsServer )
			{
				if ( RoundEndTime > 0f && Time.Now >= RoundEndTime )
				{
					RoundEndTime = 0f;
					OnTimeUp();
				}
				else
				{
					TimeLeftSeconds = TimeLeft.CeilToInt();
				}
			}
		}

		protected virtual void OnStart() { }

		protected virtual void OnFinish() { }

		protected virtual void OnTimeUp() { }
	}
}
