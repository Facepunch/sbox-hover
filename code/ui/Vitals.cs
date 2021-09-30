
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public class Vitals : Panel
	{
		public HudIconBar Energy;
		public HudIconBar Health;

		public Vitals()
		{
			Health = AddChild<HudIconBar>( "health" );
			Energy = AddChild<HudIconBar>( "energy" );
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player ) return;

			SetClass( "hidden", player.LifeState != LifeState.Alive );

			Health.InnerBar.Style.Width = Length.Fraction( Math.Max( player.Health / player.MaxHealth, 0.15f ) );
			Health.InnerBar.Style.Dirty();
			Health.Text.Text = ((int)player.Health).ToString();
			Health.SetClass( "low", player.Health < player.MaxHealth * 0.4f );

			Energy.InnerBar.Style.Width = Length.Fraction( Math.Max( player.Energy / player.MaxEnergy, 0.15f ) );
			Energy.InnerBar.Style.Dirty();
			Energy.Text.Text = ((int)player.Energy).ToString();
			Energy.SetClass( "low", player.Energy < player.MaxEnergy * 0.4f );

			base.Tick();
		}
	}
}
