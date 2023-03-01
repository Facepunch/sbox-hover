using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class PulsarConfig : WeaponConfig
	{
		public override string Name => "Pulsar";
		public override string Description => "Long-range explosive projectile based rifle";
		public override string Icon => "ui/weapons/pulsar.png";
		public override string ClassName => "hv_pulsar";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 30;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 700;
	}

	[Library( "hv_pulsar", Title = "Pulsar" )]
	partial class Pulsar : ProjectileWeapon<Projectile>
	{
		public override WeaponConfig Config => new PulsarConfig();
		public override string ProjectileData => "pulsar";
		public override string MuzzleFlashEffect => "particles/weapons/fusion_rifle/fusion_rifle_muzzleflash.vpcf";
		public override string ViewModelPath => "models/gameplay/weapons/pulsar/pulsar.vmdl";
		public override string CrosshairClass => "semiautomatic";
		public override float InheritVelocity => 0.5f;
		public override float PrimaryRate => 0.5f;
		public override float SecondaryRate => 1.0f;
		public override float Spread => 0f;
		public override string DamageType => "blast";
		public override int ClipSize => 1;
		public override bool ReloadAnimation => false;
		public override bool CanMeleeAttack => true;
		public override float ReloadTime => 1f;
		public virtual float BlastRadius => 400f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/gameplay/weapons/pulsar/w_pulsar.vmdl" );
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
			PlaySound( $"pulserifle.fire{Game.Random.Int(1, 2)}" );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		public override void PlayReloadSound()
		{
			PlaySound( "pulserifle.reload" );
			base.PlayReloadSound();
		}

		public override void SimulateAnimator( AnimationHelperWithLegs anim )
		{
			anim.HoldType = AnimationHelperWithLegs.HoldTypes.Rifle;
		}

		protected override void OnProjectileHit( Projectile projectile, Entity target )
		{
			var explosion = Particles.Create( "particles/weapons/fusion_rifle/fusion_rifle_explosion.vpcf" );
			explosion.SetPosition( 0, projectile.Position - projectile.Velocity.Normal * projectile.Data.Radius );

			ScreenShake.DoRandomShake( projectile.Position, BlastRadius, 2f );

			if ( Game.IsServer )
            {
				var position = projectile.Position;
				var entities = WeaponUtil.GetBlastEntities( position, BlastRadius );

				foreach ( var entity in entities )
				{
					var direction = (entity.Position - position).Normal;
					var distance = entity.Position.Distance( position );
					var damage = Config.Damage - ((Config.Damage / BlastRadius) * distance);

					if ( entity == Owner )
					{
						damage *= 1.25f;
					}

					DealDamage( entity, position, direction * projectile.Velocity.Length * 0.2f, damage );
				}
			}
		}
	}
}
