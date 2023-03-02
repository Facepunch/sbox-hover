using Sandbox;
using Sandbox.UI;
using Facepunch.Hover;

namespace Facepunch.Hover.UI
{
	[StyleSheet( "/ui/Crosshair.scss" )]
	public class Crosshair : Panel
	{
		public Panel ChargeBackgroundBar;
		public Panel ChargeForegroundBar;
		public Panel Charge;

		private int FireCounter;

		public Crosshair()
		{
			for ( int i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
			}

			Charge = Add.Panel( "charge" );
			ChargeBackgroundBar = Charge.Add.Panel( "background" );
			ChargeForegroundBar = ChargeBackgroundBar.Add.Panel( "foreground" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Game.LocalPawn is not HoverPlayer player )
				return;

			SetClass( "hidden", !LongshotScope.Instance.HasClass( "hidden" ) );
			Charge.SetClass( "hidden", true );

			if ( player.ActiveChild is Weapon weapon )
			{
				if ( weapon.ChargeAttackEndTime > 0f && Time.Now < weapon.ChargeAttackEndTime )
				{
					var timeLeft = weapon.ChargeAttackEndTime - Time.Now;

					ChargeForegroundBar.Style.Width = Length.Percent( 100f - ((100f / weapon.ChargeAttackDuration) * timeLeft) );
					ChargeForegroundBar.Style.Dirty();

					Charge.SetClass( "hidden", false );
				}
			}

			this.PositionAtCrosshair();

			SetClass( "fire", FireCounter > 0 );

			if ( FireCounter > 0 )
				FireCounter--;
		}

		[PanelEvent]
		public void FireEvent()
		{
			FireCounter += 2;
		}
	}
}
