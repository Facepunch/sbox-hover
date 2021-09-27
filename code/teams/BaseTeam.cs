using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public abstract class BaseTeam
	{
		public readonly List<Player> Players = new();
		public int Index { get; internal set; }

		public virtual bool HasDeployments => true;
		public virtual bool HideNameplate => false;
		public virtual string HudClassName => "";
		public virtual string Name => "";

		public void Join( Player player )
		{
			if ( player.IsLocalPawn )
			{
				Local.Hud.AddClass( HudClassName );
			}

			Players.Add( player );

			OnJoin( player );
		}

		public void Leave( Player player )
		{
			if ( player.IsLocalPawn )
			{
				Local.Hud.RemoveClass( HudClassName );
			}

			Players.Remove( player );

			OnLeave( player );
		}

		public virtual void OnTick() { }

		public virtual void OnTick( Player player ) { }

		public virtual void OnLeave( Player player  ) { }

		public virtual void OnJoin( Player player  ) { }

		public virtual void OnStart( Player player ) { }

		public virtual void OnTakeDamageFromPlayer( Player player, Player attacker, DamageInfo info ) { }

		public virtual void OnDealDamageToPlayer( Player player, Player target, DamageInfo info ) { }

		public virtual void OnPlayerKilled( Player player ) { }

		public virtual void SupplyLoadout( Player player  ) { }

		public virtual bool PlayPainSounds( Player player )
		{
			return false;
		}
	}
}
