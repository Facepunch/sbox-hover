using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class DeployableEntity : GeneratorDependency
	{
		[Net] public RealTimeUntil FinishDeployTime { get; set; }

		public virtual PhysicsMotionType MotionType => PhysicsMotionType.Keyframed;
		public virtual string ExplosionEffect => "particles/weapons/grenade_launcher/grenade_launcher_impact.vpcf";
		public virtual bool StartFrozen => false;
		public virtual string ExplosionSound => "barage.explode";
		public virtual string HealthAttachment => "health_bar";
		public virtual string DeploySound => "turret.deploy";
		public virtual bool CanPickup => true;
		public virtual float DeployTime => 2f;
		public virtual string ModelName => "";

		[Net] public float MaxHealth { get; set; } = 100f;
		[Net, Change] public bool IsDeployed { get; set; }
		[Net] public float PickupProgress { get; set; }
		[Net] public HoverPlayer Deployer { get; set; }

		private UI.WorldDeployableHud DeployableHud { get; set; }
		private UI.WorldHealthBar HealthBar { get; set; }
		private TimeSince LastUseTime { get; set; }

		public override void OnGameReset()
		{
			base.OnGameReset();

			// Delete it. Deployables don't persist across rounds.
			Delete();
		}

		public virtual void Explode()
		{
			Particles.Create( ExplosionEffect, Position );
			Audio.Play( ExplosionSound, Position );
			Delete();
		}

		public override bool OnUse( Entity user )
		{
			if ( user is HoverPlayer player && player == Deployer )
			{
				PickupProgress = Math.Min( PickupProgress + Time.Delta, 1f );
				LastUseTime = 0f;

				if ( PickupProgress == 1f )
				{
					player.OnDeployablePickedUp( this );
					PlaySound( $"weapon.pickup{Game.Random.Int( 1, 4 )}" );
					Delete();

					return false;
				}

				return true;
			}

			return base.OnUse( user );
		}

		public override bool IsUsable( Entity user )
		{
			if ( user is HoverPlayer player && player == Deployer )
            {
				return true;
            }

			return base.IsUsable( user );
		}

		public override void Spawn()
		{
			SetModel( ModelName );
			SetupPhysicsFromModel( MotionType );

			if ( StartFrozen )
			{
				PhysicsEnabled = false;
			}

			FinishDeployTime = DeployTime;
			PlaySound( DeploySound );

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			HealthBar = new UI.WorldHealthBar();
			HealthBar.MaximumValue = MaxHealth;
			HealthBar.SetEntity( this, HealthAttachment );
			HealthBar.RotateToFace = true;
			HealthBar.ShowIcon = false;

			if ( Game.LocalPawn == Deployer && CanPickup )
			{
				DeployableHud = new UI.WorldDeployableHud();
				DeployableHud.SetEntity( this );
			}

			base.ClientSpawn();
		}

		public override void OnKilled()
		{
			Explode();
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
			DeployableHud?.Delete();
			HealthBar?.Delete();

			base.OnDestroy();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( !FinishDeployTime ) return;

			if ( info.Attacker is HoverPlayer attacker )
			{
				if ( attacker.Team == Team && attacker != Deployer )
				{
					return;
				}
			}

			base.TakeDamage( info );
		}

		[GameEvent.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( HealthBar == null ) return;

			HealthBar.SetValue( Health );
			HealthBar.SetIsLow( Health < MaxHealth * 0.1f );
		}

		[GameEvent.Tick.Server]
		protected virtual void UpdateDeployment()
		{
			if ( !FinishDeployTime )
			{
				var timeLeft = FinishDeployTime.Relative;
				var fraction = 1f - (timeLeft / DeployTime);
				Health = MaxHealth * fraction;
			}
			else if ( !IsDeployed )
			{
				Health = MaxHealth;
				IsDeployed = true;
				OnDeploymentCompleted();
			}
		}

		protected virtual void OnDeploymentCompleted()
		{

		}

		protected virtual void OnIsDeployedChanged()
		{
			if ( HealthBar != null )
			{
				HealthBar.OnlyShowWhenDamaged = true;
			}
		}
	}
}
