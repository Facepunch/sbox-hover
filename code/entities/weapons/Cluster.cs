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
	partial class Cluster : ProjectileWeapon<ClusterProjectile>
	{
		public override WeaponConfig Config => new ClusterConfig();
		public override string ProjectileData => "cluster";
		public override string ViewModelPath => "models/gameplay/weapons/shotblast/shotblast.vmdl";
		public override int ViewModelMaterialGroup => 2;
		public override string MuzzleFlashEffect => "particles/weapons/cluster/cluster_muzzleflash.vpcf";
		public override string CrosshairClass => "shotgun";
		public override float InheritVelocity => 0.3f;
		public override string DamageType => "blast";
		public override float PrimaryRate => 0.5f;
		public override float SecondaryRate => 1.0f;
		public override bool CanMeleeAttack => true;
		public override int ClipSize => 1;
		public override float ReloadTime => 3f;
		public virtual float BlastRadius => 500f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/gameplay/weapons/shotblast/w_shotblast.vmdl" );
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
			var bomb = Projectile.Create<BouncingProjectile>( "bomb" );
			bomb.Bounciness = 0.5f;

			var random = new Vector3( Game.Random.Float( -1f, 1f ), Game.Random.Float( -1f, 1f ), Game.Random.Float( 0.5f, 1f ) );
			var direction = (Vector3.Up * Game.Random.Float( 0.6f, 1f )) + (random * Game.Random.Float( 1f, 1.5f ));

			bomb.Initialize( position, direction * 400f, OnBombHit );
		}

		protected virtual void OnBombHit( Projectile bomb, Entity target )
		{
			ScreenShake.DoRandomShake( bomb.Position, BlastRadius, 2f );

			DamageInRadius( bomb.Position, BlastRadius * 0.6f, Config.Damage * 4f );
		}

		protected override float ModifyDamage( Entity victim, float damage )
		{
			if ( victim == Owner ) return damage * 1.25f;

			return base.ModifyDamage( victim, damage );
		}

		protected override void OnProjectileHit( Projectile projectile, Entity target )
		{
			ScreenShake.DoRandomShake( projectile.Position, BlastRadius, 2f );

			if ( Game.IsServer )
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
