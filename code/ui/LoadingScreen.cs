
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public class LoadingScreen : Panel
	{
		public Label Text;

		public LoadingScreen()
		{
			StyleSheet.Load( "/ui/LoadingScreen.scss" );

			Text = Add.Label( "Loading", "loading" );
		}

		public override void Tick()
		{
			var isHidden = false;

			if ( Local.Pawn is Player player )
			{
				if ( player.HasTeam )
					isHidden = true;

				if ( player.IsSpectator && !player.HasSpectatorTarget )
				{
					if ( player.SpectatorDeathPosition.IsNearlyZero() )
						isHidden = false;
				}
			}

			SetClass( "hidden", isHidden );

			base.Tick();
		}
	}
}
