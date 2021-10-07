using Sandbox;

namespace Facepunch.Hover
{
	public partial class FirstPersonCamera : Camera
	{
		private Vector3 LastPosition { get; set; }

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Pos = pawn.EyePos;
			Rot = pawn.EyeRot;
			FieldOfView = 80f;
			LastPosition = Pos;
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
		}
	}
}
