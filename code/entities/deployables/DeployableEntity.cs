﻿using Gamelib.Utility;
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
		public virtual bool CanPickup => true;
		public virtual float DeployTime => 2f;
		public virtual string Model => "";

		[Net] public float PickupProgress { get; set; }
		[Net] public Player Deployer { get; set; }

		private WorldDeployableHud DeployableHud { get; set; }
		private WorldHealthBar HealthBar { get; set; }
		private TimeSince LastUseTime { get; set; }

		public override bool OnUse( Entity user )
		{
			if ( user is Player player && player == Deployer )
			{
				PickupProgress = Math.Min( PickupProgress + Time.Delta, 1f );
				LastUseTime = 0f;

				if ( PickupProgress == 1f )
				{
					player.OnDeployablePickedUp( this );
					PlaySound( $"weapon.pickup{Rand.Int( 1, 4 )}" );
					Delete();

					return false;
				}

				return true;
			}

			return base.OnUse( user );
		}

		public override bool IsUsable( Entity user )
		{
			if ( user is Player player && player == Deployer )
            {
				return true;
            }

			return base.IsUsable( user );
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

			if ( Local.Pawn == Deployer && CanPickup
				)
			{
				DeployableHud = new WorldDeployableHud();
				DeployableHud.SetEntity( this );
			}

			base.ClientSpawn();
		}

		public override void OnKilled()
		{
			Particles.Create( ExplosionEffect, Position );
			Audio.Play( ExplosionSound, Position );
			Delete();
		}

		protected override void ServerTick()
		{
			if ( LastUseTime > 0.1f )
			{
				PickupProgress = Math.Max( PickupProgress - Time.Delta * 2f, 0f );
			}

			base.ServerTick();
		}

		protected override void OnDestroy()
		{
			HealthBar?.Delete();

			base.OnDestroy();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( !FinishDeployTime ) return;

			if ( info.Attacker is Player attacker )
			{
				if ( attacker.Team == Team && attacker != Deployer )
				{
					return;
				}
			}

			base.TakeDamage( info );
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			HealthBar.SetValue( Health );
			HealthBar.SetIsLow( Health < MaxHealth * 0.1f );
		}

		[Event.Tick.Server]
		protected virtual void UpdateDeployment()
		{
			if ( !FinishDeployTime )
			{
				var timeLeft = FinishDeployTime.Relative;
				var fraction = 1f - (timeLeft / DeployTime);
				Health = MaxHealth * fraction;
			}
		}
	}
}