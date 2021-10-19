using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_claymore" )]
	public partial class Claymore : DeployableEntity, IKillFeedIcon
	{
		public override string Model => "models/claymore_mines/claymore_mines.vmdl";
		public override float MaxHealth => 80f;
		public DamageFlags DamageType { get; set; } = DamageFlags.Blast;
		public float DamageVsHeavy { get; set; } = 1f;
		public float BaseDamage { get; set; } = 700f;
		public float Force { get; set; } = 2f;
		public float Radius { get; set; } = 100f;

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

		protected override void ServerTick()
		{
			var lasers = new string[] { "laser1", "laser2" };

			for ( var i = 0; i < lasers.Length; i++ )
			{
				var name = lasers[i];
				var attachment = GetAttachment( name );

				if ( attachment.HasValue )
				{
					var position = attachment.Value.Position;
					var direction = attachment.Value.Rotation.Forward;

					var trace = Trace.Ray( position, position + direction * Radius )
						.Ignore( this )
						.Size( 2f )
						.Run();

					DebugOverlay.Line( trace.StartPos, trace.EndPos, Color.Orange.WithAlpha( 0.4f ) );

					if ( trace.Entity is Player player && IsValidVictim( player ) )
					{
						Explode( player );
						return;
					}
				}
			}

			base.ServerTick();
		}
	}
}
