using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class ViewModel : BaseViewModel
	{
		private float WalkBob { get; set; }

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			AddCameraEffects( ref camSetup );
		}

		private void AddCameraEffects( ref CameraSetup camSetup )
		{
			if ( Owner is not Player player || player.Controller is not MoveController controller )
				return;

			Rotation = Local.Pawn.EyeRot;

			var speed = Owner.Velocity.Length.LerpInverse( 0, 320 );
			var left = camSetup.Rotation.Left;
			var up = camSetup.Rotation.Up;

			if ( Owner.GroundEntity != null && !controller.IsSkiing )
			{
				WalkBob += Time.Delta * 25.0f * speed;
			}

			Position += up * MathF.Sin( WalkBob ) * speed * -1;
			Position += left * MathF.Sin( WalkBob * 0.6f ) * speed * -0.5f;
		}
	}
}
