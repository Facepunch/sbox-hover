
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class FloatingDamage : WorldPanel
	{
		public Label DamageLabel { get; private set; }
		public Vector3 Velocity { get; set; }

		private RealTimeUntil KillTime { get; set; }
		private float FadeTime { get; set; }
		private float LifeTime { get; set; }

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
			LifeTime = time + FadeTime;
			KillTime = time + FadeTime;
		}

		public override void Tick()
		{
			if ( IsDeleting ) return;

			if ( KillTime )
			{
				Delete();
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
