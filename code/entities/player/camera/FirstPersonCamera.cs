using Sandbox;

namespace Facepunch.Hover
{
	public partial class FirstPersonCamera : CameraMode
	{
		public float DefaultFieldOfView { get; set; } = 80f;
		public float TargetFieldOfView { get; set; }

		private Vector3 LastPosition { get; set; }

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Position = pawn.EyePosition;
			Rotation = pawn.EyeRotation;
			FieldOfView = DefaultFieldOfView;
			LastPosition = Position;
			TargetFieldOfView = DefaultFieldOfView;
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var eyePos = pawn.EyePosition;

			if ( eyePos.Distance( LastPosition ) < 300 )
				Position = Vector3.Lerp( eyePos.WithZ( LastPosition.z ), eyePos, 20.0f * Time.Delta );
			else
				Position = eyePos;

			Rotation = pawn.EyeRotation;
			ZNear = 4f;
			Viewer = pawn;
			LastPosition = Position;
			FieldOfView = FieldOfView.LerpTo( TargetFieldOfView, Time.Delta * 4f );
		}
	}
}
