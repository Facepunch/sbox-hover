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

		[Net] public float MaxHealth { get; set; } = 4000f;

		[Property] public Team Team { get; set; }

		private WorldHealthBar HealthBarLeft { get; set; }
		private WorldHealthBar HealthBarRight { get; set; }

		public override void Spawn()
		{
			SetModel( "models/tempmodels/generator/generator_temp.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

			Health = MaxHealth;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			HealthBarLeft = new WorldHealthBar();
			HealthBarLeft.SetEntity( this, "health_left" );
			HealthBarLeft.MaximumValue = MaxHealth;
			HealthBarLeft.WorldScale = 2f;

			HealthBarRight = new WorldHealthBar();
			HealthBarRight.SetEntity( this, "health_right" );
			HealthBarRight.MaximumValue = MaxHealth;
			HealthBarRight.WorldScale = 2f;

			base.ClientSpawn();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( info.Attacker is Player player && player.Team == Team )
			{
				// Players cannot destroy their own team's generator.
				return;
			}

			base.TakeDamage( info );
		}

		public override void OnKilled()
		{
			LifeState = LifeState.Dead;
		}

		[Event.Tick.Client]
		public virtual void ClientTick()
		{
			HealthBarLeft.SetValue( Health );
			HealthBarLeft.SetIsLow( Health < MaxHealth * 0.2f );
			HealthBarRight.SetValue( Health );
			HealthBarRight.SetIsLow( Health < MaxHealth * 0.2f );
		}
	}
}
