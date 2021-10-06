using Sandbox;

namespace Facepunch.Hover
{
	[Library( "hv_longshot", Title = "Longshot" )]
	partial class Longshot : Weapon
	{
		public override string ImpactEffect => "particles/weapons/pulse_sniper/pulse_sniper_impact.vpcf";
		public override string TracerEffect => "particles/weapons/pulse_sniper/pulse_sniper_projectile.vpcf";
		public override string ViewModelPath => "weapons/rust_shotgun/v_rust_shotgun.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/pulse_sniper/pulse_sniper_muzzleflash.vpcf";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_smg.png" );
		public override string WeaponName => "Longshot";
		public override float PrimaryRate => 0.5f;
		public override float SecondaryRate => 1.0f;
		public override int Slot => 3;
		public override int ClipSize => 5;
		public override bool ReloadAnimation => false;
		public override float ReloadTime => 4f;
		public override int BaseDamage => 300;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_shotgun/rust_shotgun.vmdl" );
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
			ShootBullet( 0f, 2f, BaseDamage, 3.0f );

			AnimationOwner.SetAnimBool( "b_attack", true );

			if ( AmmoClip == 0 )
				PlaySound( "pulserifle.empty" );

			TimeSincePrimaryAttack = 0;
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
	}
}
