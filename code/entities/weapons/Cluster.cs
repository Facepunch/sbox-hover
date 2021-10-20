using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class ClusterConfig : WeaponConfig
	{
		public override string Name => "Cluster";
		public override string Description => "Medium-range cluster bomb launcher";
		public override string Icon => "ui/weapons/cluster.png";
		public override string ClassName => "hv_cluster";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override int Ammo => 15;
	}

	[Library( "hv_cluster", Title = "Cluster" )]
	partial class Cluster : PhysicsWeapon<BoomerProjectile>
	{
		public override WeaponConfig Config => new ClusterConfig();
		public override string ImpactEffect => "particles/weapons/cluster/cluster_impact.vpcf";
		public override string TrailEffect => "particles/weapons/cluster/cluster_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_shotblast.vmdl";
		public override int ViewModelMaterialGroup => 2;
		public override string MuzzleFlashEffect => "particles/weapons/cluster/cluster_muzzleflash.vpcf";
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override float InheritVelocity => 0.3f;
		public override float PrimaryRate => 0.5f;
		public override float SecondaryRate => 1.0f;
		public override float ProjectileForce => 60f;
		public override bool CanMeleeAttack => true;
		public override string ProjectileModel => "models/weapons/barage_grenade/barage_grenade.vmdl";
		public override float ImpactForce => 1000f;
		public override int ClipSize => 1;
		public override float DamageRadius => 350f;
		public override float ReloadTime => 3f;
		public override float LifeTime => 2f;
		public override int BaseDamage => 250;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_shotblast.vmdl" );
			SetMaterialGroup( 2 );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"barage.launch" );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		public override void PlayReloadSound()
		{
			PlaySound( "blaster.reload" );
			base.PlayReloadSound();
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		protected virtual void CreateBomb( Vector3 position )
		{
			var bomb = new PhysicsProjectile()
			{
				ExplosionEffect = ImpactEffect,
				TrailEffect = TrailEffect,
				HitSound = HitSound,
				LifeTime = Rand.Float( 1f, 2.5f ),
				Owner = Owner
			};
			
			bomb.SetModel( ProjectileModel );

			var random = new Vector3( Rand.Float( -1f, 1f ), Rand.Float( -1f, 1f ), Rand.Float( -1f, 1f ) );

			bomb.Position = position;
			bomb.Rotation = Rotation.From( Angles.Random );
			bomb.Initialize( OnBombHit );
			bomb.PhysicsBody.ApplyForce( (Vector3.Up * ProjectileForce * Rand.Float( 60f, 100f )) + (random * ProjectileForce * Rand.Float( 100f, 150f )) );
		}

		protected virtual void OnBombHit( PhysicsProjectile bomb )
		{
			DamageInRadius( bomb.Position, DamageRadius * 0.7f, BaseDamage * 4f );
		}

		protected override void OnProjectileHit( PhysicsProjectile projectile )
		{
			base.OnProjectileHit( projectile );

			PlaySound( $"barage.launch" );

			for ( var i = 0; i < 5; i++ )
			{
				CreateBomb( projectile.Position );
			}
		}
	}
}
