using Sandbox.UI.Construct;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Facepunch.Hover
{
	[UseTemplate]
	public class Radar : Panel
	{
		private readonly Dictionary<HoverPlayer, RadarDot> RadarDots = new();

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is not HoverPlayer localPlayer )
				return;

			SetClass( "hidden", localPlayer.LifeState != LifeState.Alive );

			var deleteList = new List<HoverPlayer>();
			var count = 0;

			deleteList.AddRange( RadarDots.Keys );

			var players = Entity.All.OfType<HoverPlayer>().OrderBy( x => Vector3.DistanceBetween( x.EyePosition, Camera.Position ) );

			foreach ( var v in players )
			{
				if ( UpdateRadar( v ) )
				{
					deleteList.Remove( v );
					count++;
				}
			}

			foreach ( var player in deleteList )
			{
				RadarDots[player].Delete();
				RadarDots.Remove( player );
			}
		}

		public RadarDot CreateRadarDot( HoverPlayer player )
		{
			var tag = new RadarDot( player )
			{
				Parent = this
			};

			return tag;
		}

		public bool UpdateRadar( HoverPlayer player )
		{
			if ( player.IsLocalPawn || !player.HasTeam )
				return false;

			if ( player.LifeState != LifeState.Alive )
				return false;

			if ( player.ShouldHideOnRadar )
				return false;

			if ( Local.Pawn is not HoverPlayer localPlayer )
				return false;

			if ( player.Team == localPlayer.Team )
				return false;

			var radarRange = 8000f;

			if ( player.Position.Distance( localPlayer.Position ) > radarRange )
				return false;

			if ( !RadarDots.TryGetValue( player, out var dot ) )
			{
				dot = CreateRadarDot( player );
				RadarDots[player] = dot;
			}

			// This is probably fucking awful maths but it works.
			var difference = player.Position - localPlayer.Position;
			var radarSize = 256f;

			var x = (radarSize / radarRange) * difference.x * 0.5f;
			var y = (radarSize / radarRange) * difference.y * 0.5f;

			var angle = (MathF.PI / 180) * (Camera.Rotation.Yaw() - 90f);
			var x2 = x * MathF.Cos( angle ) + y * MathF.Sin( angle );
			var y2 = y * MathF.Cos( angle ) - x * MathF.Sin( angle );

			dot.SetClass( "enemy", player.Team != localPlayer.Team );

			dot.Style.Left = (radarSize / 2f) + x2;
			dot.Style.Top = (radarSize / 2f) - y2;
			dot.Style.Dirty();

			return true;
		}
	}
}
