using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_blaster", Title = "Blaster" )]
	partial class Blaster : BulletDropWeapon
	{
		public override float ProjectileRadius => 30f;
		public override string ImpactEffect => "particles/weapons/blaster/blaster_impact.vpcf";
		public override string TrailEffect => "particles/weapons/blaster/blaster_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_blaster.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/blaster/blaster_muzzleflash.vpcf";
		public override Texture Icon => Texture.Load( "ui/weapons/blaster.png" );
		public override string CrosshairClass => "automatic";
		public override float ProjectileLifeTime => 1f;
		public override string WeaponName => "Blaster";
		public override float PrimaryRate => 7.5f;
		public override float DamageFalloffStart => 1000f;
		public override float DamageFalloffEnd => 3000f;
		public override AmmoType AmmoType => AmmoType.SMG;
		public override float SecondaryRate => 1.0f;
		public override bool CanMeleeAttack => false;
		public override int Slot => 1;
		public override int ClipSize => 30;
		public override float Spread => 0.05f;
		public override bool ReloadAnimation => true;
		public override float ReloadTime => 3f;
		public override int BaseDamage => 60;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_blaster.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			ShootEffects();
			PlaySound( $"blaster.fire1" );

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
			PlaySound( "blaster.reload" );
			base.PlayReloadSound();
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
