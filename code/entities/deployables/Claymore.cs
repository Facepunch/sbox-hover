using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_claymore" )]
	public partial class Claymore : DeployableEntity, IKillFeedIcon
	{
		private class ClaymoreLaser
		{
			public Particles Effect { get; set; }
			public string Attachment { get; set; }
			public Claymore Claymore { get; set; }

			public ClaymoreLaser( Claymore claymore, string attachment )
			{
				Attachment = attachment;
				Claymore = claymore;
				Effect = Particles.Create( "particles/claymore_mines/claymore_mines.vpcf", claymore, attachment );
				Effect.SetPosition( 2, Color.Orange * 255f );
			}

			public void Update()
			{
				var attachment = GetAttachment();
				Effect?.SetPosition( 1, attachment.Position + attachment.Rotation.Forward * Claymore.Radius );
			}

			public Transform GetAttachment()
			{
				return Claymore.GetAttachment( Attachment ).Value;
			}

			public void Destroy()
			{
				Effect?.Destroy();
			}
		}

		public override string ModelName => "models/claymore_mines/claymore_mines.vmdl";
		public override bool RequiresPower => false;
		public DamageFlags DamageType { get; set; } = DamageFlags.Blast;
		public float DamageVsHeavy { get; set; } = 1f;
		public float BaseDamage { get; set; } = 700f;
		public float Force { get; set; } = 2f;
		public float Radius { get; set; } = 100f;

		private List<ClaymoreLaser> Lasers { get; set; } = new();

		public override void Spawn()
		{
			MaxHealth = 80f;

			base.Spawn();
		}

		public string GetKillFeedIcon()
		{
			return "ui/killicons/claymore.png";
		}

		public Team GetKillFeedTeam()
		{
			return Team;
		}

		public string GetKillFeedName()
		{
			return "Claymore";
		}

		public override void Explode()
		{
			DealDamage( Position, Rotation.Forward * 100f * Force, BaseDamage );

			base.Explode();
		}

		protected virtual void CreateLasers()
		{
			var lasers = new string[] { "laser1", "laser2" };

			for ( var i = 0; i < lasers.Length; i++ )
			{
				var laser = new ClaymoreLaser( this, lasers[i] );
				laser.Update();
				Lasers.Add( laser );
			}
		}

		protected virtual void DealDamage( Vector3 position, Vector3 force, float damage )
		{
			var players = WeaponUtil.GetBlastEntities<Player>( position, Radius * 1.25f )
				.Where( IsValidVictim );

			foreach ( var player in players )
			{
				var direction = (player.Position - position).Normal;

				if ( direction.Dot( Rotation.Forward ) < 0.7f )
					continue;

				var damageInfo = new DamageInfo()
					.WithAttacker( Deployer )
					.WithWeapon( this )
					.WithPosition( position )
					.WithForce( force )
					.WithFlag( DamageType );

				damageInfo.Damage = damage;

				if ( player.Loadout.ArmorType == LoadoutArmorType.Heavy )
				{
					damageInfo.Damage *= DamageVsHeavy;
				}

				player.TakeDamage( damageInfo );
			}
		}

		protected virtual bool IsValidTarget( Player victim )
		{
			return (victim.LifeState == LifeState.Alive && victim.Team != Team);
		}

		protected virtual bool IsValidVictim( Player victim )
		{
			return (victim.LifeState == LifeState.Alive && (victim == Deployer || victim.Team != Team));
		}

		protected override void OnDestroy()
		{
			foreach ( var laser in Lasers )
			{
				laser.Destroy();
			}

			base.OnDestroy();
		}

		protected override void OnDeploymentCompleted()
		{
			CreateLasers();

			base.OnDeploymentCompleted();
		}

		protected override void ServerTick()
		{
			for ( var i = 0; i < Lasers.Count; i++ )
			{
				var laser = Lasers[i];
				var attachment = laser.GetAttachment();
				var position = attachment.Position;
				var direction = attachment.Rotation.Forward;

				var trace = Trace.Ray( position, position + direction * Radius )
					.Ignore( this )
					.Size( 2f )
					.Run();

				if ( trace.Entity is Player player && IsValidTarget( player ) )
				{
					Explode();
					return;
				}
			}

			base.ServerTick();
		}
	}
}
