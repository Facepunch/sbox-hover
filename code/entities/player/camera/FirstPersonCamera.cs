using Sandbox;

namespace Facepunch.Hover
{
	public partial class FirstPersonCamera
	{
		private Vector3 LastPosition { get; set; }

		public void Update()
		{
			var player = Local.Pawn as Player;
			if ( player == null ) return;

			var eyePos = player.EyePosition;

			if ( eyePos.Distance( LastPosition ) < 300 )
				Camera.Position = Vector3.Lerp( eyePos.WithZ( LastPosition.z ), eyePos, 20.0f * Time.Delta );
			else
				Camera.Position = eyePos;

			Camera.FirstPersonViewer = player;
			Camera.Rotation = player.EyeRotation;
			Camera.ZNear = 4f;

			if ( player.ActiveChild is Longshot longshot && longshot.IsScoped )
				Camera.FieldOfView = Camera.FieldOfView.LerpTo( 10f, Time.Delta * 4f );
			else
				Camera.FieldOfView = Camera.FieldOfView.LerpTo( 90f, Time.Delta * 4f );

			LastPosition = Camera.Position;
		}
	}
}
