using Sandbox;
using System.Collections.Generic;

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

		public override string Model => "models/claymore_mines/claymore_mines.vmdl";
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

		protected virtual void DealDamage( Player target, Vector3 position, Vector3 force, float damage )
		{
			if ( target is Player player && player.Loadout.ArmorType == LoadoutArmorType.Heavy )
			{
				damage *= DamageVsHeavy;
			}

			var damageInfo = new DamageInfo()
				.WithAttacker( Deployer )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( force )
				.WithFlag( DamageType );

			damageInfo.Damage = damage;

			target.TakeDamage( damageInfo );
		}

		protected virtual bool IsValidVictim( Player victim )
		{
			return (victim.LifeState == LifeState.Alive && victim.Team != Team);
		}

		protected virtual void Explode( Player target )
		{
			var force = (target.Position - Position).Normal * 100f * Force;
			DealDamage( target, Position, force, BaseDamage );
			OnKilled();
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

				if ( trace.Entity is Player player && IsValidVictim( player ) )
				{
					Explode( player );
					return;
				}
			}

			base.ServerTick();
		}
	}
}
