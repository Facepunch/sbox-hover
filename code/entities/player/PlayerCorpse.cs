using Sandbox;

namespace Facepunch.Hover
{
	public class PlayerCorpse : ModelEntity
	{
		public HoverPlayer Player { get; set; }

		public PlayerCorpse()
		{
			UsePhysicsCollision = true;
			PhysicsEnabled = true;
		}

		public override void Spawn()
		{
			Tags.Add( "corpse" );
			base.Spawn();
		}

		public void CopyFrom( HoverPlayer player )
		{
			RenderColor = player.RenderColor;

			SetModel( player.GetModelName() );
			TakeDecalsFrom( player );

			// We have to use `this` to refer to the extension methods.
			this.CopyBonesFrom( player );
			this.SetRagdollVelocityFrom( player );

			foreach ( var child in player.Children )
			{
				if ( child is BaseClothing e )
				{
					var model = e.GetModelName();
					var clothing = new ModelEntity();

					clothing.SetMaterialGroup( player.Team == Team.Red ? "red" : "blue" );
					clothing.RenderColor = e.RenderColor;
					clothing.SetModel( model );
					clothing.SetParent( this, true );
				}
			}
		}

		public void ApplyForceToBone( Vector3 force, int forceBone )
		{
			PhysicsGroup.AddVelocity( force );

			if ( forceBone >= 0 )
			{
				var body = GetBonePhysicsBody( forceBone );

				if ( body != null )
				{
					body.ApplyForce( force * 1000 );
				}
				else
				{
					PhysicsGroup.AddVelocity( force );
				}
			}
		}
	}
}
