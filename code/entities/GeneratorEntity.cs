using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library( "hv_generator" )]
	[Hammer.EditorModel( "models/tempmodels/generator/generator_temp.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Generator", "Hover", "Defines a point where a team's generator spawns" )]
	public partial class GeneratorEntity : ModelEntity
	{
		public delegate void GeneratorEvent( GeneratorEntity generator );
		public static event GeneratorEvent OnGeneratorRepaired;
		public static event GeneratorEvent OnGeneratorBroken;

		[Property] public Team Team { get; set; }

		public override void Spawn()
		{
			SetModel( "models/tempmodels/generator/generator_temp.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

			Health = 2000f;

			base.Spawn();
		}

		public override void OnKilled()
		{
			LifeState = LifeState.Dead;
		}
	}
}
