﻿using Sandbox;
using Sandbox.UI;
using Facepunch.Hover;

namespace Facepunch.Hover.UI
{
	[StyleSheet( "/ui/Crosshair.scss" )]
	public class Crosshair : Panel
	{
		private Panel ReloadIndicator { get; }
		private Panel ChargeBackgroundBar { get; }
		private Panel ChargeForegroundBar { get; }
		private Panel Charge { get; }

		private int FireCounter;

		public Crosshair()
		{
			for ( var i = 0; i < 5; i++ )
			{
				var p = Add.Panel( "element" );
				p.AddClass( $"el{i}" );
			}

			Charge = Add.Panel( "charge" );
			ReloadIndicator = Add.Panel( "reload" );
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
					Charge.SetClass( "hidden", false );
				}
			}

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
