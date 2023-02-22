using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_force_shield" )]
	public partial class ForceShield : DeployableEntity, IKillFeedIcon
	{
		public override string ModelName => "models/force_field/force_field.vmdl";

		public string DamageType { get; set; } = "shock";
		public float FullDamage { get; set; } = 700f;

		private RealTimeUntil LastDamageSound { get; set; }
		private RealTimeUntil NextDamageTime { get; set; }
		private RealTimeUntil NextPassSound { get; set; }
		private Particles Effect { get; set; }
		private Sound IdleSound { get; set; }

		public string GetKillFeedIcon()
		{
			return "ui/equipment/force_shield.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Force Shield";
		}

		public override void Spawn()
		{
			base.Spawn();

			Tags.Add( "blastproof" );
			Tags.Add( "passplayers" );

			Scale = 0.2f;
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( IsDeployed && LastDamageSound )
			{
				PlaySound( "forceshield.impact" )
					.SetRandomPitch( 0.5f, 1f )
					.SetVolume( Game.Random.Float( 0.8f, 1f ) );

				LastDamageSound = 0.2f;
			}

			base.TakeDamage( info );
		}

		protected virtual void DealDamage( HoverPlayer target, Vector3 position, float force, float damage )
		{
			damage = target.Velocity.Length.Remap( 0f, 2000f, 0f, damage );

			var direction = (position - target.Position).Normal;
			var damageInfo = new DamageInfo()
				.WithAttacker( Deployer )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( direction * 100f * force )
				.WithTag( DamageType );

			damageInfo.Damage = damage;

			target.TakeDamage( damageInfo );

			target.ShouldHideOnRadar = 2f;
		}

		protected virtual bool IsValidVictim( HoverPlayer player )
		{
			return (player.LifeState == LifeState.Alive && player.Team != Team);
		}

		protected override void ServerTick()
		{
			if ( IsPowered && IsDeployed && NextDamageTime )
			{
				var players = FindInBox( WorldSpaceBounds )
					.OfType<HoverPlayer>();

				var didDamagePlayer = false;

				foreach ( var player in players )
				{
					if ( IsValidVictim( player ) )
					{
						if ( !didDamagePlayer )
						{
							Particles.Create( "particles/generator/generator_attacked/generator_attacked.vpcf", this );
							PlaySound( "forceshield.hurtplayer" );
							PlaySound( "forceshield.zap" );
							didDamagePlayer = true;
						}

						DealDamage( player, Position, 2f, FullDamage );
					}
					else if ( NextPassSound )
					{
						NextPassSound = 1f;
						PlaySound( "forceshield.move" );
					}
				}

				if ( didDamagePlayer )
				{
					NextDamageTime = 1f;
				}
			}

			base.ServerTick();
		}

		protected override void OnDestroy()
		{
			DestroyParticleEffect();
			StopIdleSound();

			base.OnDestroy();
		}

		protected override void OnDeploymentCompleted()
		{
			if ( IsPowered )
			{
				PlaySound( "forceshield.pass" );
				CreateParticleEffect();
				StartIdleSound();
			}
		}

		protected override void OnGeneratorRepaired( GeneratorAsset generator )
		{
			CreateParticleEffect();
			StartIdleSound();

			base.OnGeneratorRepaired( generator );
		}

		protected override void OnGeneratorBroken( GeneratorAsset generator )
		{
			DestroyParticleEffect();
			StopIdleSound();

			base.OnGeneratorBroken( generator );
		}

		private void StartIdleSound()
		{
			StopIdleSound();

			IdleSound = PlaySound( "forceshield.idle" );
		}

		private void StopIdleSound()
		{
			IdleSound.Stop();
		}

		private void DestroyParticleEffect()
		{
			Effect?.Destroy();
			Effect = null;
		}

		private void CreateParticleEffect()
		{
			DestroyParticleEffect();

			Effect = Particles.Create( "particles/force_field/force_field.vpcf", this );
			Effect.SetEntity( 0, this );
			Effect.SetPosition( 1, Team.GetColor() * 255f );
		}
	}
}
