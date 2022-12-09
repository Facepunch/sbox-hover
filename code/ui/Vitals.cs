using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover.UI
{
	public class Vitals : Panel
	{
		public HudIconBar Energy;
		public HudIconBar Health;
		public Panel Divider;

		public Vitals()
		{
			Health = AddChild<HudIconBar>( "health" );
			Divider = Add.Panel( "divider" );
			Energy = AddChild<HudIconBar>( "energy" );
		}

		private RealTimeUntil HealthGrowTime { get; set; }
		private RealTimeUntil EnergyGrowTime { get; set; }
		private float LastHealth { get; set; }
		private float LastEnergy { get; set; }

		public override void Tick()
		{
			if ( Local.Pawn is not HoverPlayer player ) return;

			SetClass( "hidden", player.LifeState != LifeState.Alive );

			var health = player.Health;

			Health.InnerBar.Style.Width = Length.Fraction( Math.Max( health / player.MaxHealth, 0.05f ) );
			Health.InnerBar.Style.Dirty();
			Health.Text.Text = ((int)health).ToString();
			Health.SetClass( "low", health < player.MaxHealth * 0.25f );

			Energy.InnerBar.Style.Width = Length.Fraction( Math.Max( player.Energy / player.MaxEnergy, 0.05f ) );
			Energy.InnerBar.Style.Dirty();
			Energy.Text.Text = ((int)player.Energy).ToString();
			Energy.SetClass( "low", player.Energy < player.MaxEnergy * 0.25f );

			if ( player.Energy != LastEnergy && EnergyGrowTime )
			{
				EnergyGrowTime = 0.1f;
			}

			Energy.SetClass( "grow", !EnergyGrowTime );
			LastEnergy = player.Energy;

			if ( health != LastHealth && HealthGrowTime )
			{
				HealthGrowTime = 0.1f;
			}

			Health.SetClass( "grow", !HealthGrowTime );
			LastHealth = health;

			base.Tick();
		}
	}
}
