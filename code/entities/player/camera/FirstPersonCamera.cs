using Sandbox;

namespace Facepunch.Hover
{
	public partial class FirstPersonCamera
	{
		private Vector3 LastPosition { get; set; }

		public void Update()
		{
			if ( Game.LocalPawn is not HoverPlayer player ) return;

			var eyePos = player.EyePosition;

			Camera.Position = eyePos.Distance( LastPosition ) < 300f ? Vector3.Lerp( eyePos.WithZ( LastPosition.z ), eyePos, 20f * Time.Delta ) : eyePos;

			Camera.FirstPersonViewer = player;
			Camera.Rotation = player.EyeRotation;
			Camera.ZNear = 4f;

			var targetDefaultFov = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

			Camera.FieldOfView = Camera.FieldOfView.LerpTo( player.ActiveChild is Longshot { IsScoped: true } ? 10f : targetDefaultFov, Time.Delta * 4f );

			ScreenShake.Apply();

			LastPosition = Camera.Position;
		}
	}
}
