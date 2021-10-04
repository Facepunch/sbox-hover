using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library( "hv_launch_pad" )]
	[Hammer.RenderFields]
	[Hammer.EditorModel( "models/launch_pad/launch_pad.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Launch Pad", "Hover", "A pad that launches players toward a target entity" )]
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
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			if ( Force == 0f )
			{
				Force = 1000f;
			}

			base.Spawn();
		}

		public override void StartTouch( Entity other )
		{
			if ( other is Player player && player.IsAuthority )
			{
				var target = FindByName( TargetEntity );

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
