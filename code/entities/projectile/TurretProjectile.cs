using Sandbox;

namespace Facepunch.Hover
{
	[Library]
	public partial class TurretProjectile : Projectile
	{
		public float MoveTowardTarget { get; set; }
		public ModelEntity Target { get; set; }

		protected override Vector3 GetTargetPosition()
		{
			var newPosition = base.GetTargetPosition();

			if ( Target.IsValid() && MoveTowardTarget > 0f )
			{
				var targetDirection = (Target.WorldSpaceBounds.Center - newPosition).Normal;
				newPosition += targetDirection * MoveTowardTarget * Time.Delta;
			}

			return newPosition;
		}
	}
}
