using Sandbox;
using System;

namespace Facepunch.Hover
{
	public struct ViewModelAimConfig
	{
		public float Speed { get; set; }
		public Angles Rotation { get; set; }
		public Vector3 Position { get; set; }
		public bool AutoHide { get; set; }
	}

	public partial class ViewModel : BaseViewModel
	{
		public ViewModelAimConfig AimConfig { get; set; }
		public bool IsAiming { get; private set; }

		private RealTimeUntil? AimHideTime { get; set; }
		private Vector3 PositionOffset { get; set; }
		private Angles RotationOffset { get; set; }
		private float WalkBob { get; set; }

		public ViewModel() : base()
		{
			AimConfig = new ViewModelAimConfig
			{
				Speed = 1f
			};
		}

		public void SetIsAiming( bool isAiming, RealTimeUntil? hideTime = null )
		{
			AimHideTime = hideTime;
			IsAiming = isAiming;
		}

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

			if ( IsAiming )
			{
				PositionOffset = PositionOffset.LerpTo( AimConfig.Position, Time.Delta * AimConfig.Speed );
				RotationOffset = Angles.Lerp( RotationOffset, AimConfig.Rotation, Time.Delta * AimConfig.Speed );

				if ( AimHideTime.HasValue && AimHideTime.Value )
					EnableDrawing = false;
				else
					EnableDrawing = true;
			}
			else
			{
				PositionOffset = PositionOffset.LerpTo( Vector3.Zero, Time.Delta * AimConfig.Speed );
				RotationOffset = Angles.Lerp( RotationOffset, Angles.Zero, Time.Delta * AimConfig.Speed );

				if ( AimConfig.AutoHide )
				{
					EnableDrawing = true;
				}
			}

			Position += Rotation.Forward * PositionOffset.x + Rotation.Left * PositionOffset.y + Rotation.Up * PositionOffset.z;

			var angles = Rotation.Angles();
			angles += RotationOffset;
			Rotation = angles.ToRotation();

			Position += up * MathF.Sin( WalkBob ) * speed * -1;
			Position += left * MathF.Sin( WalkBob * 0.6f ) * speed * -0.5f;
		}
	}
}
