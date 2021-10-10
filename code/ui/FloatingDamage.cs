
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
		private float LifeTime { get; set; }

		public FloatingDamage()
		{
			StyleSheet.Load( "/ui/FloatingDamage.scss" );
			DamageLabel = Add.Label( "0", "damage" );
		}

		public void SetDamage( float damage )
		{
			DamageLabel.Text = damage.CeilToInt().ToString();
		}

		public void SetLifeTime( float time )
		{
			LifeTime = time;
			KillTime = time;
		}

		public override void Tick()
		{
			if ( IsDeleting ) return;

			if ( KillTime )
			{
				Delete();
				return;
			}

			var opacity = (1f / LifeTime) * KillTime;

			Position += Velocity * Time.Delta;
			Velocity -= Velocity * Time.Delta;

			Style.Opacity = opacity;
			Style.Dirty();

			Rotation = Rotation.LookAt( CurrentView.Position - Position );

			base.Tick();
		}
	}
}
