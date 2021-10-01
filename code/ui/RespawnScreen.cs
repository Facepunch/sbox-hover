
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public class RespawnScreen : Panel
	{
		public Label RespawnTime { get; private set; }

		public RespawnScreen()
		{
			StyleSheet.Load( "/ui/RespawnScreen.scss" );

			RespawnTime = Add.Label( "", "respawn" );
		}

		public override void Tick()
		{
			var isHidden = false;

			RespawnTime.SetClass( "hidden", true );

			if ( Local.Pawn is Player player )
			{
				if ( player.HasTeam )
					isHidden = true;

				if ( player.IsSpectator )
				{
					isHidden = false;

					RespawnTime.SetClass( "hidden", false );
					RespawnTime.Text = $"Respawn in {player.RespawnTime.Relative.CeilToInt()}";
				}
			}

			SetClass( "hidden", isHidden );

			base.Tick();
		}
	}
}
