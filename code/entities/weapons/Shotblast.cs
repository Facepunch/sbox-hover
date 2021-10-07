using Sandbox;


namespace Facepunch.Hover
{
	[Library( "hv_shotblast", Title = "Shotblast" )]
	partial class Shotblast : Weapon
	{
		public override string ViewModelPath => "models/weapons/v_shotblast.vmdl";
		public override Texture Icon => Texture.Load( "ui/weapons/dm_shotgun.png" );
		public override string WeaponName => "Shotblast";
		public override float PrimaryRate => 1;
		public override float SecondaryRate => 1;
		public override AmmoType AmmoType => AmmoType.Shotgun;
		public override int ClipSize => 4;
		public override float ReloadTime => 2f;
		public override bool CanMeleeAttack => false;
		public override int BaseDamage => 50;
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
			PlaySound( "rust_pumpshotgun.shoot" );

			for ( int i = 0; i < 10; i++ )
			{
				ShootBullet( 0.15f, 0.3f, BaseDamage, 3.0f );
			}

			AnimationOwner.SetAnimBool( "b_attack", true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
