using Sandbox.UI.Construct;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Facepunch.Hover
{
	public class Nameplate : WorldPanel
	{
		public Player Player { get; private set; }
		public Panel Container { get; private set; }
		public Panel Dot { get; private set; }
		public Label NameLabel { get; private set; }
		public float StartFadeDistance { get; set; } = 1500f;
		public float EndFadeDistance { get; set; } = 2000f;
		public float StartDotDistance { get; set; } = 2250f;
		public float EndDotDistance { get; set; } = 5000f;

		public Nameplate( Player player )
		{
			StyleSheet.Load( "/ui/Nameplate.scss" );
			Container = Add.Panel( "container" );
			Dot = Add.Panel( "dot" );
			NameLabel = Container.Add.Label( "", "name" );
			PanelBounds = new Rect( -1000, -1000, 2000, 2000 );
			Player = player;
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player localPlayer )
				return;

			if ( IsDeleting ) return;

			if ( !Player.IsValid() )
			{
				Delete();
				return;
			}

			if ( !IsPlayerVisible() )
			{
				SetClass( "hidden", true );
				return;
			}

			var transform = Transform;

			transform.Position = Player.WorldSpaceBounds.Center + Vector3.Up * 60f;

			var targetRotation = Rotation.LookAt( CurrentView.Position - Position );
			transform.Rotation = targetRotation;

			var distanceToCamera = localPlayer.Position.Distance( Player.Position );
			transform.Scale = distanceToCamera.Remap( 0f, EndFadeDistance, 1f, 3f );

			if ( distanceToCamera >= StartFadeDistance )
			{
				var overlap = (distanceToCamera - StartFadeDistance);
				var opacity = overlap.Remap( 0f, (EndFadeDistance - StartFadeDistance), 0f, 1f );
				Container.Style.Opacity = Math.Clamp( 1f - opacity, 0f, 1f );
			}
			else
			{
				Container.Style.Opacity = 1f;
			}

			if ( Player.Team != localPlayer.Team )
			{
				if ( !Player.VisibleToEnemiesUntil && distanceToCamera >= StartDotDistance && distanceToCamera < EndDotDistance )
				{
					var halfDistance = StartDotDistance + (EndDotDistance - StartDotDistance) * 0.5f;
					var overlap = (distanceToCamera - StartDotDistance);

					if ( distanceToCamera >= halfDistance )
					{
						var opacity = overlap.Remap( 0f, halfDistance, 0f, 1f );
						Dot.Style.Opacity = Math.Clamp( opacity, 0f, 1f );
					}
					else
					{
						var opacity = overlap.Remap( 0f, halfDistance, 0f, 1f );
						Dot.Style.Opacity = Math.Clamp( 1f - opacity, 0f, 1f );
					}
				}
				else
				{
					Dot.Style.Opacity = 0f;
				}
			}
			else
			{
				Dot.Style.Opacity = 1f;
			}

			NameLabel.Text = Player.Client.Name;

			Transform = transform;

			SetClass( Team.Blue.GetHudClass(), Player.Team == Team.Blue );
			SetClass( Team.Red.GetHudClass(), Player.Team == Team.Red );
			SetClass( "hidden", Dot.Style.Opacity == 0f && Container.Style.Opacity == 0f );

			base.Tick();
		}

		private bool IsPlayerVisible()
		{
			if ( Player.LifeState == LifeState.Dead )
				return false;

			if ( Player.TargetAlpha == 0f )
				return false;

			return true;
		}
	}
}
