using Sandbox;

namespace Facepunch.Hover
{
	public partial class SpectateCamera : Camera
	{
		private Vector3 FocusPoint { get; set; }

		public override void Activated()
		{
			base.Activated();

			FocusPoint = CurrentView.Position - GetViewOffset();
			FieldOfView = 70;
		}

		public override void Update()
		{
			if ( Local.Pawn is not Player player )
				return;

			FocusPoint = Vector3.Lerp( FocusPoint, GetSpectatePoint(), Time.Delta * 5.0f );

			Pos = FocusPoint + GetViewOffset();
			Rot = player.EyeRot;

			FieldOfView = FieldOfView.LerpTo( 50, Time.Delta * 3.0f );
			Viewer = null;
		}

		private Vector3 GetSpectatePoint()
		{
			if ( Local.Pawn is Player player )
				return player.DeathPosition;
			else
				return Vector3.Up * 5000f;
		}

		private Vector3 GetViewOffset()
		{
			if ( Local.Pawn is not Player player )
				return Vector3.Zero;

			return player.EyeRot.Forward * -150 + Vector3.Up * 10;
		}
	}
}
