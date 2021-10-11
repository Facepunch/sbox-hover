using Sandbox;

namespace Facepunch.Hover
{
	[Library]
	public class VehicleController : PawnController
	{
		public override void FrameSimulate()
		{
			base.FrameSimulate();

			Simulate();
		}

		public override void Simulate()
		{
			var player = Pawn as Player;
			if ( !player.IsValid() ) return;

			var car = player.Vehicle as VehicleEntity;
			if ( !car.IsValid() ) return;

			car.Simulate( Client );

			if ( player.Vehicle == null )
			{
				Position = car.Position + car.Rotation.Up * (100f * car.Scale);
				Velocity += car.Rotation.Right * (200f * car.Scale);
				return;
			}

			EyeRot = Input.Rotation;
			EyePosLocal = Vector3.Up * (64f - 10f) * car.Scale;
			Velocity = car.Velocity;
			
			SetTag( "noclip" );
			SetTag( "sitting" );
		}
	}
}
