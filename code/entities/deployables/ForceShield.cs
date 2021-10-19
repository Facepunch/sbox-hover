using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_force_shield" )]
	public partial class ForceShield : DeployableEntity, IKillFeedIcon
	{
		public override string Model => "models/force_field/force_field.vmdl";

		public DamageFlags DamageType { get; set; } = DamageFlags.Shock;
		public float FullDamage { get; set; } = 700f;

		private RealTimeUntil NextDamageTime { get; set; }
		private RealTimeUntil NextPassSound { get; set; }

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
			return "Force Field";
		}

		public override void Spawn()
		{
			base.Spawn();

			CollisionGroup = CollisionGroup.Default;
			SetInteractsAs( CollisionLayer.Water );
			SetInteractsWith( CollisionLayer.Solid );

			Tags.Add( "blastproof" );

			Scale = 0.2f;
		}

		protected virtual void DealDamage( Player target, Vector3 position, float force, float damage )
		{
			damage = target.Velocity.Length.Remap( 0f, 2000f, 0f, damage );

			var direction = (position - target.Position).Normal;
			var damageInfo = new DamageInfo()
				.WithAttacker( Deployer )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( direction * 100f * force )
				.WithFlag( DamageType );

			damageInfo.Damage = damage;

			target.TakeDamage( damageInfo );

			target.VisibleToEnemiesUntil = 2f;
		}

		protected virtual bool IsValidVictim( Player player )
		{
			return (player.LifeState == LifeState.Alive && player.Team != Team);
		}

		protected override void ServerTick()
		{
			if ( IsDeployed && NextDamageTime )
			{
				var players = Physics.GetEntitiesInBox( WorldSpaceBounds )
					.OfType<Player>();

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
						PlaySound( "forceshield.pass" );
					}
				}

				if ( didDamagePlayer )
				{
					NextDamageTime = 1f;
				}
			}

			base.ServerTick();
		}

		protected override void OnGeneratorRepaired( GeneratorAsset generator )
		{
			// TODO: Show shield particles.
			base.OnGeneratorRepaired( generator );
		}

		protected override void OnGeneratorBroken( GeneratorAsset generator )
		{
			// TODO: Hide shield particles.
			base.OnGeneratorBroken( generator );
		}
	}
}
