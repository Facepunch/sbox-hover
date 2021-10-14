using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_radar_sensor" )]
	[Hammer.EditorModel( "models/tempmodels/turret/turret.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Radar Sensor", "Hover", "Defines a point where a sensor spawns" )]
	public partial class RadarSensorEntity : GeneratorDependency
	{
		public override void Spawn()
		{
			SetModel( "models/tempmodels/turret/turret.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else if ( Team == Team.Red )
				RenderColor = Color.Red;

			Name = "Radar Sensor";

			base.Spawn();
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
		}

		protected override void OnIsPoweredChanged( bool isPowered )
		{
			if ( isPowered )
				SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 1f ) );
			else
				SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 0f ) );

			base.OnIsPoweredChanged( isPowered );
		}

		[Event.Tick.Server]
		private void ServerTick() { }
	}
}
