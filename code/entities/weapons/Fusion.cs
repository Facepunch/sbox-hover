﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class FusionConfig : WeaponConfig
	{
		public override string Name => "Fusion";
		public override string Description => "Medium-range hitscan LMG";
		public override string ClassName => "hv_fusion";
		public override string Icon => "ui/weapons/fusion.png";
		public override AmmoType AmmoType => AmmoType.LMG;
		public override int Ammo => 0;
	}

	[Library( "hv_fusion", Title = "Fusion" )]
	partial class Fusion : Weapon
	{
		public override WeaponConfig Config => new FusionConfig();
		public override string ImpactEffect => "particles/weapons/fusion/fusion_impact.vpcf";
		public override string TracerEffect => "particles/weapons/fusion/fusion_projectile.vpcf";
		public override string MuzzleFlashEffect => "particles/weapons/fusion/fusion_muzzleflash.vpcf";
		public override int ViewModelMaterialGroup => 3;
		public override string ViewModelPath => "models/weapons/v_blaster.vmdl";
		public override List<Type> Upgrades => new()
		{
			typeof( FusionAmmoUpgrade ),
			typeof( FusionSpinUpUpgrade ),
			typeof( FusionAmmoUpgrade ),
			typeof( FusionSpinUpUpgrade )
		};
		public override string CrosshairClass => "automatic";
		public override int ClipSize => 150;
		public override float PrimaryRate => 12f;
		public override float DamageFalloffStart => 2000f;
		public override float DamageFalloffEnd => 8000f;
		public override float SecondaryRate => 1.0f;
		public override float ReloadTime => 3.0f;
		public override int BaseDamage => 40;
		public override bool CanMeleeAttack => false;

		[Net] public float SpinUpTime { get; set; } = 1.4f;

		private Particles ChargeParticles { get; set; }
		private TimeSince SpinUpStarted { get; set; }
		private bool IsSpinningUp { get; set; }
		private Sound ChargeSound { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_blaster.vmdl" );
			SetMaterialGroup( 3 );
		}

		public override void PlayReloadSound()
		{
			PlaySound( "blaster.reload" );
			base.PlayReloadSound();
		}

		public override void AttackPrimary()
		{
			if ( !IsSpinningUp || SpinUpStarted < SpinUpTime )
			{
				if ( !IsSpinningUp )
				{
					ShowChargeParticles();
					PlayChargeSound();
					IsSpinningUp = true;
					SpinUpStarted = 0f;
				}

				return;
			}
			else
			{
				StopChargeSound();
			}

			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			TimeSincePrimaryAttack = 0f;

			Rand.SetSeed( Time.Tick );

			ShootEffects();
			PlaySound( $"blaster.fire1" );
			ShootBullet( 0.05f, 1.5f, BaseDamage, 4.0f );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );
		}

		public override void Simulate( Client owner )
		{
			if ( IsSpinningUp && !Input.Down( InputButton.Attack1 ) )
			{
				HideChargeParticles();
				StopChargeSound();
				IsSpinningUp = false;
			}

			base.Simulate( owner );
		}

		public override void ActiveEnd( Entity owner, bool dropped )
		{
			HideChargeParticles();
			StopChargeSound();

			IsSpinningUp = false;

			base.ActiveEnd( owner, dropped );
		}

		protected override void OnDestroy()
		{
			HideChargeParticles();
			StopChargeSound();

			base.OnDestroy();
		}

		private void PlayChargeSound()
		{
			StopChargeSound();
			ChargeSound = PlaySound( "fusion.charge" );
		}

		private void StopChargeSound()
		{
			ChargeSound.Stop();
		}

		private void HideChargeParticles()
		{
			ChargeParticles?.Destroy();
			ChargeParticles = null;
		}

		private void ShowChargeParticles()
		{
			HideChargeParticles();

			ChargeParticles = Particles.Create( "particles/weapons/fusion/fusion_charge.vpcf", EffectEntity, "muzzle" );
		}
	}
}
