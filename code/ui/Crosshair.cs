using Sandbox;
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
		
		private float ReloadProgress { get; set; }

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

			var isReloading = false;
			
			if ( player.ActiveChild is Weapon weapon )
			{
				if ( weapon.ChargeAttackEndTime > 0f && Time.Now < weapon.ChargeAttackEndTime )
				{
					var timeLeft = weapon.ChargeAttackEndTime - Time.Now;

					ChargeForegroundBar.Style.Width = Length.Percent( 100f - ((100f / weapon.ChargeAttackDuration) * timeLeft) );
					Charge.SetClass( "hidden", false );
				}

				if ( weapon.IsReloading )
				{
					var progress = ((weapon.TimeSinceReload / weapon.ReloadTime) * 100f).Clamp( 0f, 100f );
					var gradient = 100f - progress;
					var teamColor = player.Team.GetColor().Hex;
					
					ReloadIndicator.Style.Set( "background-image", $"conic-gradient( {teamColor} {gradient + 10}%, {teamColor} {gradient + 9}%, {teamColor} {gradient}% )" );
					
					var opacity = 1f;

					if ( progress >= 90f )
						opacity = (1f / 10f) * (100f - progress);
					
					ReloadIndicator.Style.Set( "opacity", $"{opacity}" );
					isReloading = true;
				}
			}

			SetClass( "fire", FireCounter > 0 );
			
			ReloadIndicator.SetClass( "hidden", !isReloading );
			SetClass( "reloading", isReloading );

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
