using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class DeployableEntity : GeneratorDependency
	{
		[Net] public RealTimeUntil FinishDeployTime { get; set; }

		public virtual string ExplosionEffect => "particles/weapons/grenade_launcher/grenade_launcher_impact.vpcf";
		public virtual string ExplosionSound => "barage.explode";
		public virtual string HealthAttachment => "health_bar";
		public virtual float MaxHealth => 100f;
		public virtual string DeploySound => "turret.deploy";
		public virtual float DeployTime => 2f;
		public virtual string Model => "";

		[Net] public Player Deployer { get; set; }

		private WorldHealthBar HealthBar { get; set; }

		public void SetTeam( Team team )
		{
			Team = team;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;
		}

		public override void Spawn()
		{
			SetModel( Model );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			FinishDeployTime = DeployTime;
			PlaySound( DeploySound );

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			HealthBar = new WorldHealthBar();
			HealthBar.MaximumValue = MaxHealth;
			HealthBar.SetEntity( this, HealthAttachment );
			HealthBar.RotateToFace = true;
			HealthBar.ShowIcon = false;

			Log.Info( "Attaching to: " + HealthAttachment );

			base.ClientSpawn();
		}

		public override void OnKilled()
		{
			Particles.Create( ExplosionEffect, Position );
			Audio.Play( ExplosionSound, Position );
			Delete();
		}

		protected override void OnDestroy()
		{
			HealthBar?.Delete();

			base.OnDestroy();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( !FinishDeployTime ) return;

			base.TakeDamage( info );
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			HealthBar.SetValue( Health );
			HealthBar.SetIsLow( Health < MaxHealth * 0.1f );
		}
	}
}
