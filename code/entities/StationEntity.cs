﻿using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_station" )]
	[Hammer.EditorModel( "models/upgrade_station/upgrade_station.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Station", "Hover", "Defines a point where a team's station spawns" )]
	public partial class StationEntity : GeneratorDependency, IUse
	{
		public Particles IdleParticles { get; private set; }

		public override void Spawn()
		{
			SetModel( "models/upgrade_station/upgrade_station.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else if ( Team == Team.Red )
				RenderColor = Color.Red;
			else if ( Team == Team.None )
				RenderColor = Color.Yellow;

			EnableTouch = true;
			Name = "Station";

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			CreateIdleParticles();

			base.ClientSpawn();
		}

		public override void OnKilled()
		{
			// TODO: Can it be killed separately to the generator?
		}

		protected override void OnIsPoweredChanged( bool isPowered )
		{
			if ( isPowered )
				CreateIdleParticles();
			else
				DestroyIdleParticles();

			base.OnIsPoweredChanged( isPowered );
		}

		private void CreateIdleParticles()
		{
			IdleParticles?.Destroy();
			IdleParticles = Particles.Create( "particles/upgrade_station/upgrade_idle", this );
		}

		private void DestroyIdleParticles()
		{
			IdleParticles?.Destroy();
			IdleParticles = null;
		}

		public bool OnUse( Entity user )
		{
			if ( user is Player player )
			{
				StationScreen.Show( To.Single( player ) );
				Particles.Create( "particles/upgrade_station/upgrade_use", this );
			}

			return false;
		}

		public bool IsUsable( Entity user )
		{
			return true;
		}
	}
}
