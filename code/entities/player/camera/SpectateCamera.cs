using Sandbox;

namespace Facepunch.Hover
{
	public partial class SpectateCamera : Camera
	{
		[Net] public Vector3 DeathPosition { get; set; }

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
			return DeathPosition;
		}

		private Vector3 GetViewOffset()
		{
			if ( Local.Pawn is not Player player )
				return Vector3.Zero;

			return player.EyeRot.Forward * -150 + Vector3.Up * 10;
		}
	}
}
