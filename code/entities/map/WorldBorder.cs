using Sandbox;
using SandboxEditor;

namespace Facepunch.Hover
{
	[Library( "hv_world_border" )]
	[AutoApplyMaterial( "materials/map_border.vmat" )]
	[Solid]
	[HammerEntity]
	public partial class WorldBorder : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			Transmit = TransmitType.Always;
		}

		public override void ClientSpawn()
		{
			var sceneObject = SceneObject;

			sceneObject.Flags.IsOpaque = false;
			sceneObject.Flags.IsTranslucent = true;
			sceneObject.Flags.IsDecal = false;
			sceneObject.Flags.OverlayLayer = false;
			sceneObject.Flags.BloomLayer = false;
			sceneObject.Flags.ViewModelLayer = false;
			sceneObject.Flags.SkyBoxLayer = false;
			sceneObject.Flags.NeedsLightProbe = false;

			base.ClientSpawn();
		}
	}
}
