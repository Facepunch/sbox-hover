using Sandbox;

namespace Facepunch.Hover
{
	public partial class FirstPersonCamera : Camera
	{
		public float DefaultFieldOfView { get; set; } = 80f;
		public float TargetFieldOfView { get; set; }

		private Vector3 LastPosition { get; set; }

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;
			FieldOfView = DefaultFieldOfView;
			LastPosition = Pos;
			TargetFieldOfView = DefaultFieldOfView;
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var eyePos = pawn.EyePos;

			if ( eyePos.Distance( LastPosition ) < 300 )
				Pos = Vector3.Lerp( eyePos.WithZ( LastPosition.z ), eyePos, 20.0f * Time.Delta );
			else
				Pos = eyePos;

			Rot = pawn.EyeRot;
			ZNear = 4f;
			Viewer = pawn;
			LastPosition = Pos;
			FieldOfView = FieldOfView.LerpTo( TargetFieldOfView, Time.Delta * 4f );
		}
	}
}
