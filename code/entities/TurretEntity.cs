using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library( "hv_turret" )]
	[Hammer.EditorModel( "models/tempmodels/generator/generator_temp.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Turret", "Hover", "Defines a point where a team's turret spawns" )]
	public partial class TurretEntity : AnimEntity
	{
		[Property] public Team Team { get; set; }

		public override void Spawn()
		{
			SetModel( "models/tempmodels/turret/turret.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

			base.Spawn();
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
		}
	}
}
