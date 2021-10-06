using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_blaser", Title = "Blaster" )]
	partial class Blaster : ProjectileWeapon
	{
		public override string ImpactEffect => "particles/weapons/blaster/blaster_impact.vpcf";
		public override string TrailEffect => "particles/weapons/blaster/blaster_projectile.vpcf";
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/blaster/blaster_muzzleflash.vpcf";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_smg.png" );
		public override string WeaponName => "Blaster";
		public override float PrimaryRate => 5.0f;
		public override float SecondaryRate => 1.0f;
		public override int Slot => 1;
		public override int ClipSize => 30;
		public override bool ReloadAnimation => false;
		public override float ReloadTime => 2f;
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
