using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_blaser", Title = "Blaster" )]
	partial class Blaster : Weapon
	{
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/blaster/blaster_muzzleflash.vpcf";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_smg.png" );
		public override string WeaponName => "Blaster";
		public override float PrimaryRate => 10.0f;
		public override float SecondaryRate => 1.0f;
		public override int Slot => 1;
		public override int ClipSize => 30;
		public override float ReloadTime => 4.0f;
		public override bool HasFlashlight => true;
		public override bool HasLaserDot => true;
		public override int BaseDamage => 40;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			ShootEffects();
			PlaySound( "rust_smg.shoot" );

			if ( IsServer )
			{
				using ( Prediction.Off() )
				{
					FireProjectile();
				}
			}

			AnimationOwner.SetAnimBool( "b_attack", true );
		}

		public virtual void FireProjectile()
		{
			var inheritVelocity = Owner.Velocity;
			var projectile = new PhysicsProjectile()
			{
				IgnoreEntity = this,
				TrailEffect = "particles/weapons/blaster/blaster_projectile.vpcf",
				LifeTime = 10f,
				Gravity = 50f
			};

			var muzzle = GetAttachment( "muzzle" );
			var position = muzzle.Value.Position;
			var forward = Owner.EyeRot.Forward;
			var spread = 0.02f;
			var speed = 2000f;

			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			projectile.Initialize( position, forward, 20f, speed, OnProjectileHit );
		}

		private void OnProjectileHit( PhysicsProjectile projectile, Entity target )
		{
			
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
