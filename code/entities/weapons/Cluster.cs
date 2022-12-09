using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class ClusterConfig : WeaponConfig
	{
		public override string Name => "Cluster";
		public override string Description => "Medium-range cluster bomb launcher";
		public override string Icon => "ui/weapons/cluster.png";
		public override string ClassName => "hv_cluster";
		public override AmmoType AmmoType => AmmoType.Grenade;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 15;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 250;
	}

	[Library( "hv_cluster", Title = "Cluster" )]
	partial class Cluster : BulletDropWeapon<ClusterProjectile>
	{
		public override WeaponConfig Config => new ClusterConfig();
		public override string ImpactEffect => "particles/weapons/cluster/cluster_impact.vpcf";
		public override string TrailEffect => "particles/weapons/cluster/cluster_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_shotblast.vmdl";
		public override int ViewModelMaterialGroup => 2;
		public override string MuzzleFlashEffect => "particles/weapons/cluster/cluster_muzzleflash.vpcf";
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override float InheritVelocity => 0.3f;
		public override string DamageType => "blast";
		public override float PrimaryRate => 0.5f;
		public override float SecondaryRate => 1.0f;
		public override float ProjectileLifeTime => 2f;
		public override float Gravity => 35f;
		public override float Speed => 2000f;
		public override bool CanMeleeAttack => true;
		public override string ProjectileModel => "models/weapons/barage_grenade/barage_grenade.vmdl";
		public override int ClipSize => 1;
		public override float ReloadTime => 3f;
		public virtual float BlastRadius => 500f;

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

		public override void SimulateAnimator( AnimationHelperWithLegs anim )
		{
			anim.HoldType = AnimationHelperWithLegs.HoldTypes.Rifle;
		}

		protected virtual void CreateBomb( Vector3 position )
		{
			var bomb = new BouncingProjectile()
			{
				ExplosionEffect = ImpactEffect,
				TrailEffect = TrailEffect,
				Bounciness = 0.5f,
				HitSound = HitSound,
				LifeTime = Rand.Float( 0.5f, 1.5f ),
				Gravity = 50f
			};
			
			bomb.SetModel( ProjectileModel );

			var random = new Vector3( Rand.Float( -1f, 1f ), Rand.Float( -1f, 1f ), Rand.Float( 0.5f, 1f ) );
			var direction = (Vector3.Up * Rand.Float( 0.6f, 1f )) + (random * Rand.Float( 1f, 1.5f ));

			bomb.Initialize( position, direction * 400f, 8f, OnBombHit );
		}

		protected virtual void OnBombHit( BulletDropProjectile bomb, Entity target )
		{
			DamageInRadius( bomb.Position, BlastRadius * 0.6f, Config.Damage * 4f );
		}

		protected override float ModifyDamage( Entity victim, float damage )
		{
			if ( victim == Owner ) return damage * 1.25f;

			return base.ModifyDamage( victim, damage );
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			if ( IsServer )
			{
				DamageInRadius( projectile.Position, BlastRadius, Config.Damage, 4f );

				var position = projectile.Position;

				Audio.Play( "barage.launch", position );

				for ( var i = 0; i < 5; i++ )
				{
					CreateBomb( position );
				}
			}
		}
	}
}
