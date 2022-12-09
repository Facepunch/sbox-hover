using Sandbox;

namespace Facepunch.Hover
{
	partial class HoverPlayer
	{
		public PlayerCorpse Ragdoll { get; set; }

		[ClientRpc]
		private void BecomeRagdollOnClient( Vector3 force, int forceBone )
		{
			var ragdoll = new PlayerCorpse
			{
				Position = Position,
				Rotation = Rotation
			};

			ragdoll.CopyFrom( this );
			ragdoll.ApplyForceToBone( force, forceBone );
			ragdoll.Player = this;
			ragdoll.PhysicsEnabled = true;

			Ragdoll = ragdoll;
		}

		private void BecomeRagdollOnServer( Vector3 force, int forceBone )
		{
			var ragdoll = new PlayerCorpse
			{
				Position = Position,
				Rotation = Rotation
			};

			ragdoll.CopyFrom( this );
			ragdoll.ApplyForceToBone( force, forceBone );
			ragdoll.Player = this;

			Ragdoll = ragdoll;
		}
	}
}
