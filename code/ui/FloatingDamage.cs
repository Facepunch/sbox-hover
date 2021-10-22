
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class FloatingDamage : WorldPanel
	{
		private static Queue<FloatingDamage> Pool { get; set; } = new();

		public static FloatingDamage Rent()
		{
			if ( Pool.Count == 0 )
			{
				return new FloatingDamage();
			}

			var panel = Pool.Dequeue();

			panel.SceneObject.RenderingEnabled = true;
			panel.Style.Opacity = 1f;
			panel.SetClass( "hidden", false );
			panel.IsPooled = false;

			return panel;
		}

		public static void Return( FloatingDamage panel )
		{
			panel.SceneObject.RenderingEnabled = false;
			panel.SetClass( "hidden", true );
			panel.IsPooled = true;

			Pool.Enqueue( panel );
		}

		public Label DamageLabel { get; private set; }
		public Vector3 Velocity { get; set; }
		public bool IsPooled { get; private set; }

		private RealTimeUntil KillTime { get; set; }
		private float FadeTime { get; set; }

		public FloatingDamage()
		{
			StyleSheet.Load( "/ui/FloatingDamage.scss" );
			DamageLabel = Add.Label( "0", "damage" );
			PanelBounds = new Rect( -1000f, -1000f, 2000f, 2000f );
		}

		public void SetDamage( float damage )
		{
			DamageLabel.Text = damage.CeilToInt().ToString();
		}

		public void SetLifeTime( float time )
		{
			FadeTime = 0.5f;
			KillTime = time + FadeTime;
		}

		public override void Tick()
		{
			if ( IsPooled ) return;

			if ( KillTime )
			{
				Return( this );
				return;
			}

			if ( KillTime < FadeTime )
			{
				var opacity = (1f / FadeTime) * KillTime;
				Style.Opacity = opacity;
			}

			Position += Velocity * Time.Delta;
			Velocity -= Velocity * Time.Delta;

			Rotation = Rotation.LookAt( CurrentView.Position - Position );
			WorldScale = Position.Distance( CurrentView.Position ).Remap( 0f, 10000f, 2.5f, 5f );

			base.Tick();
		}
	}
}
