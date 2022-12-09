using Sandbox;
using Editor;

namespace Facepunch.Hover
{
	[Library( "hv_launch_pad" )]
	[RenderFields]
	[EditorModel( "models/launch_pad/launch_pad.vmdl", FixedBounds = true )]
	[Title( "Launch Pad" )]
	[Line( "targetname", "targetentity" )]
	[HammerEntity]
	public partial class LaunchPad : ModelEntity
	{
		[Net, Property, FGDType( "target_destination" )] public string TargetEntity { get; set; } = "";
		[Net, Property] public float VerticalBoost { get; set; } = 200f;
		[Net, Property] public float Force { get; set; } = 1000f;

		public LaunchPad()
		{
			EnableAllCollisions = false;
			EnableTouch = true;
		}

		public override void Spawn()
		{
			SetModel( "models/launch_pad/launch_pad.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			if ( Force == 0f )
			{
				Force = 1000f;
			}
			Tags.Add( "trigger" );
			base.Spawn();
		}

		public override void ClientSpawn()
		{
			var particles = Particles.Create( "particles/launch_pad/launch_pad_horizontal.vpcf", this );
			particles.SetPosition( 1, RenderColor * 255f );

			base.ClientSpawn();
		}

		public override void StartTouch( Entity other )
		{
			if ( other is HoverPlayer player && player.IsAuthority )
			{
				var target = FindByName( TargetEntity );

				if ( IsServer )
				{
					var effect = Particles.Create( "particles/launch_pad/launch_pad_horizontal_jump.vpcf", this );
					effect.SetPosition( 1, RenderColor * 255f );
				}

				if ( target.IsValid() )
				{
					var direction = (target.Position - player.Position).Normal;
					player.ApplyForce( new Vector3( 0f, 0f, VerticalBoost ) );
					player.ApplyForce( direction * Force );
				}
				else
				{
					player.ApplyForce( new Vector3( 0f, 0f, Force ) );
				}
			}

			base.StartTouch( other );
		}
	}
}
