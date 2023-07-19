using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class BigPulsarConfig : WeaponConfig
	{
		public override string Name => "Big Pulsar";
		public override string Description => "Medium-range explosive projectile based rifle";
		public override string SecondaryDescription => "+50% vs Structures";
		public override string Icon => "ui/weapons/big_pulsar.png";
		public override string ClassName => "hv_big_pulsar";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 30;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 800;
	}

	[Library( "hv_big_pulsar", Title = "Big Pulsar" )]
	partial class BigPulsar : Pulsar
	{
		public override WeaponConfig Config => new BigPulsarConfig();
		public override int ViewModelMaterialGroup => 1;
		public override string ProjectileData => "bigpulsar";
		public override float InheritVelocity => 0.5f;
		public override string ViewModelPath => "models/gameplay/weapons/pulsar/pulsar.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/big_pulsar/big_pulsar_muzzleflash.vpcf";
		public override float DamageFalloffStart => 1500f;
		public override float DamageFalloffEnd => 5000f;
		public override float ReloadTime => 1.2f;

		public override void Spawn()
		{
			base.Spawn();

			SetMaterialGroup( 1 );
		}

		protected override void OnProjectileHit( Projectile projectile, Entity target )
		{
			var explosion = Particles.Create( "particles/weapons/big_pulsar/big_pulsar_explosion.vpcf" );
			explosion.SetPosition( 0, projectile.Position - projectile.Velocity.Normal * projectile.Data.Radius );

			ScreenShake.DoRandomShake( projectile.Position, BlastRadius, 2f );

			if ( Game.IsServer )
            {
				var position = projectile.Position;
				var entities = WeaponUtil.GetBlastEntities( position, BlastRadius );
				var fullDistance = position.Distance( projectile.StartPosition );

				foreach ( var entity in entities )
				{
					var direction = (entity.Position - position).Normal;
					var distance = entity.Position.Distance( position );
					var damage = Config.Damage - ((Config.Damage / BlastRadius) * distance);

					if ( entity is GeneratorAsset)
						damage *= 1.25f;
					else if ( entity == Owner )
						damage *= 0.75f;

					damage = GetDamageFalloff( fullDistance, damage );

					DealDamage( entity, position, direction * projectile.Velocity.Length * 0.2f, damage );
				}
			}
		}
	}
}
