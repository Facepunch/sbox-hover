using Sandbox;
using System;

namespace Facepunch.Hover
{
	[Library( "hv_knife", Title = "Knife" )]
	partial class Knife : Weapon
	{
		public override string ViewModelPath => "weapons/rust_boneknife/v_rust_boneknife.vmdl";
		public override float PrimaryRate => 1.0f;
		public override float SecondaryRate => 0.3f;
		public override bool IsMelee => true;
		public override int HoldType => 0;
		public override int Slot => 3;
		public override int BaseDamage => 35;
		public virtual int MeleeDistance => 80;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_boneknife/rust_boneknife.vmdl" );
		}

		public override void AttackSecondary()
		{
			StartChargeAttack();
		}

		public override void AttackPrimary()
		{
			ShootEffects();
			PlaySound( "rust_boneknife.attack" );
			MeleeStrike( BaseDamage, 1.5f );
		}

		public override void OnChargeAttackFinish()
		{
			ShootEffects();
			PlaySound( "rust_boneknife.attack" );
			MeleeStrike( BaseDamage * 3f, 1.5f );
		}
	}
}
