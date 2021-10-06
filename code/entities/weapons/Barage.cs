using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_barage", Title = "Barage" )]
	partial class Barage : ProjectileWeapon
	{
		public override string ImpactEffect => "particles/weapons/blaster/blaster_impact.vpcf";
		public override string TrailEffect => "particles/weapons/blaster/blaster_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_barage.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/blaster/blaster_muzzleflash.vpcf";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_smg.png" );
		public override string WeaponName => "Barage";
		public override float PrimaryRate => 5.0f;
		public override float SecondaryRate => 1.0f;
		public override int Slot => 0;
		public override int ClipSize => 30;
		public override bool ReloadAnimation => true;
		public override float ReloadTime => 3f;
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
