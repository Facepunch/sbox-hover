using Sandbox;

namespace Facepunch.Hover
{
	public partial class AssaultLoadout : BaseLoadout
	{
		protected ModelEntity JetpackEntity { get; set; }
		protected Particles JetpackParticles { get; set; }

		public override void SupplyLoadout()
		{
			Entity.ClearAmmo();
			Entity.Inventory.DeleteContents();
			Entity.Inventory.Add( new Pistol(), true );
			Entity.Inventory.Add( new SMG(), true );
			Entity.GiveAmmo( AmmoType.Pistol, 120 );
		}

		public override void Setup()
		{
			Entity.ClearAmmo();
			Entity.Inventory.DeleteContents();
			Entity.SetModel( "models/citizen/citizen.vmdl" );

			Entity.RemoveClothing();
			Entity.AttachClothing( "models/citizen_clothes/trousers/trousers.lab.vmdl" );
			Entity.AttachClothing( "models/citizen_clothes/jacket/labcoat.vmdl" );
			Entity.AttachClothing( "models/citizen_clothes/shoes/shoes.workboots.vmdl" );
			Entity.AttachClothing( "models/citizen_clothes/hat/hat_securityhelmet.vmdl" );

			JetpackEntity = Entity.AttachClothing( "models/tempmodels/jetpack/jetpack.vmdl" );

			Entity.RemoveAllDecals();

			Entity.EnableAllCollisions = true;
			Entity.EnableDrawing = true;
			Entity.EnableHideInFirstPerson = true;
			Entity.EnableShadowInFirstPerson = true;

			Entity.Controller = new MoveController();
			Entity.Camera = new FirstPersonCamera();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( Entity.Controller is not MoveController controller )
				return;

			if ( controller.IsJetpacking )
			{
				if ( JetpackParticles == null )
				{
					JetpackParticles = Particles.Create( "particles/jetpack/jetpack_trail.vpcf" );
					JetpackParticles.SetEntityAttachment( 0, JetpackEntity, "trail" );
				}
			}
			else if ( JetpackParticles != null )
			{
				JetpackParticles.Destroy();
				JetpackParticles = null;
			}
	}
}
