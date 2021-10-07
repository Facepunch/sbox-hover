using Sandbox;


namespace Facepunch.Hover
{
	[Library( "hv_shotblast", Title = "Shotblast" )]
	partial class Shotblast : Weapon
	{
		public override string ImpactEffect => "particles/weapons/shotblast/shotblast_impact.vpcf";
		public override string TracerEffect => "particles/weapons/shotblast/shotblast_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/shotblast/shotblast_muzzleflash.vpcf";
		public override string ViewModelPath => "models/weapons/v_shotblast.vmdl";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_shotgun.png" );
		public override float DamageFalloffStart => 0f;
		public override float DamageFalloffEnd => 1000f;
		public override string WeaponName => "Shotblast";
		public override float PrimaryRate => 1;
		public override float SecondaryRate => 1;
		public override AmmoType AmmoType => AmmoType.Shotgun;
		public override int ClipSize => 4;
		public override float ReloadTime => 2f;
		public override bool CanMeleeAttack => false;
		public override int BaseDamage => 40;
		public override int Slot => 5;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_shotblast.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			ShootEffects();
			PlaySound( $"shotblast.fire{Rand.Int(1, 2)}" );

			for ( int i = 0; i < 6; i++ )
			{
				ShootBullet( 0.5f, 3f, BaseDamage, 3.0f );
			}

			AnimationOwner.SetAnimBool( "b_attack", true );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
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
