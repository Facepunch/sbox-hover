﻿using Sandbox;
using System;
using System.Collections.Generic;
using Gamelib.Utility;

namespace Facepunch.Hover
{
	public class BigPulsarConfig : WeaponConfig
	{
		public override string Name => "Big Pulsar";
		public override string Description => "Medium-range explosive projectile based rifle";
		public override string SecondaryDescription => "+50% vs Structures";
		public override string Icon => "ui/weapons/big_pulsar.png";
		public override string ClassName => "hv_big_pulsar";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override int Ammo => 30;
	}

	[Library( "hv_big_pulsar", Title = "Big Pulsar" )]
	partial class BigPulsar : Pulsar
	{
		public override WeaponConfig Config => new BigPulsarConfig();
		public override string ImpactEffect => "particles/weapons/big_pulsar/big_pulsar_impact.vpcf";
		public override string TrailEffect => "particles/weapons/big_pulsar/big_pulsar_projectile.vpcf";
		public override int ViewModelMaterialGroup => 1;
		public override float InheritVelocity => 0.5f;
		public override string ViewModelPath => "models/weapons/v_pulsar.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/big_pulsar/big_pulsar_muzzleflash.vpcf";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override float DamageFalloffStart => 1500f;
		public override float DamageFalloffEnd => 5000f;
		public override float ProjectileLifeTime => 5f;
		public override float Speed => 3000f;
		public override float ReloadTime => 1.2f;
		public override int BaseDamage => 800;

		public override void Spawn()
		{
			base.Spawn();

			SetMaterialGroup( 1 );
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			var explosion = Particles.Create( "particles/weapons/big_pulsar/big_pulsar_explosion.vpcf" );
			explosion.SetPosition( 0, projectile.Position - projectile.Velocity.Normal * projectile.Radius );

			var position = projectile.Position;
			var entities = WeaponUtil.GetBlastEntities( position, BlastRadius );
			var fullDistance = position.Distance( projectile.StartPosition );
			
			foreach ( var entity in entities )
			{
				var direction = (entity.Position - position).Normal;
				var distance = entity.Position.Distance( position );
				var damage = BaseDamage - ((BaseDamage / BlastRadius) * distance);

				if ( entity is GeneratorAsset )
				{
					damage *= 1.5f;
				}

				damage = GetDamageFalloff( fullDistance, damage );

				DealDamage( entity, position, direction * projectile.Velocity.Length * 0.25f, damage );
			}
		}
	}
}
