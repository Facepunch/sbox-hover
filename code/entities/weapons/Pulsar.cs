using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_pulsar", Title = "Pulsar" )]
	partial class Pulsar : BulletDropWeapon
	{
		public override string ImpactEffect => "particles/weapons/fusion_rifle/fusion_rifle_impact.vpcf";
		public override string TrailEffect => "particles/weapons/fusion_rifle/fusion_rifle_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_pulsar.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/fusion_rifle/fusion_rifle_muzzleflash.vpcf";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_smg.png" );
		public override string WeaponName => "Pulsar";
		public override string HitSound => "barage.explode";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override float PrimaryRate => 1.0f;
		public override float SecondaryRate => 1.0f;
		public override float Speed => 3000f;
		public override DamageFlags DamageType => DamageFlags.Blast;
		public override int Slot => 4;
		public override int ClipSize => 1;
		public override bool ReloadAnimation => false;
		public override bool CanMeleeAttack => false;
		public override float ReloadTime => 1f;
		public override int BaseDamage => 300;
		public virtual float BlastRadius => 500f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_pulsar.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			ShootEffects();
			PlaySound( $"pulserifle.fire{Rand.Int(1, 2)}" );

			AnimationOwner.SetAnimBool( "b_attack", true );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		public override bool CanReload()
		{
			return (TimeSincePrimaryAttack > 1f && AmmoClip == 0) || base.CanReload();
		}

		public override void PlayReloadSound()
		{
			PlaySound( "pulserifle.reload" );
			base.PlayReloadSound();
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			var explosion = Particles.Create( "particles/weapons/fusion_rifle/fusion_rifle_explosion.vpcf" );
			explosion.SetPosition( 0, projectile.Position );

			var position = projectile.Position;
			var entities = Physics.GetEntitiesInSphere( position, BlastRadius );
			
			foreach ( var entity in entities )
			{
				var direction = (entity.Position - position).Normal;
				var distance = entity.Position.Distance( position );
				var damage = BaseDamage - ((BaseDamage / BlastRadius) * distance);
				DealDamage( entity, position, direction * projectile.Speed * 0.25f, damage );
			}
		}
	}
}
