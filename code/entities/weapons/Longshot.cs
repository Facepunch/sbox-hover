using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library]
	public class LongshotConfig : WeaponConfig
	{
		public override string Name => "Longshot";
		public override string Description => "Long-range hitscan sniper rifle";
		public override string Icon => "ui/weapons/longshot.png";
		public override string ClassName => "hv_longshot";
		public override AmmoType AmmoType => AmmoType.Rifle;
		public override WeaponType Type => WeaponType.Hitscan;
		public override int Ammo => 20;
		public override List<Type> Upgrades => new()
		{
			typeof( AmmoPackUpgrade ),
			typeof( DamageVsHeavy ),
			typeof( AmmoPackUpgrade )
		};
		public override int Damage => 350;
	}

	[Library( "hv_longshot", Title = "Longshot" )]
	partial class Longshot : Weapon
	{
		public override WeaponConfig Config => new LongshotConfig();
		public override string ImpactEffect => "particles/weapons/pulse_sniper/pulse_sniper_impact.vpcf";
		public override string TracerEffect => "particles/weapons/pulse_sniper/pulse_sniper_projectile.vpcf";
		public override string ViewModelPath => "models/gameplay/weapons/longshot/longshot.vmdl";
		public override string MuzzleFlashEffect => "particles/weapons/pulse_sniper/pulse_sniper_muzzleflash.vpcf";
		public override string CrosshairClass => "semiautomatic";
		public override float PrimaryRate => 0.3f;
		public override float SecondaryRate => 1.0f;
		public override bool CanMeleeAttack => true;
		public override float DamageFalloffStart => 15000f;
		public override float DamageFalloffEnd => 25000f;
		public override int ClipSize => 3;
		public override float ReloadTime => 4f;

		public bool IsScoped { get; private set; }

		public void SetScoped( bool isScoped )
		{
			IsScoped = isScoped;

			if ( isScoped )
				UI.LongshotScope.Instance.Show();
			else
				UI.LongshotScope.Instance.Hide();

			if ( ViewModelEntity is ViewModel viewModel )
			{
				viewModel.SetIsAiming( isScoped, 0.15f );
			}
		}

		public override void ActiveEnd( Entity owner, bool dropped )
		{
			if ( IsScoped )
			{
				SetScoped( false );
			}

			base.ActiveEnd( owner, dropped );
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/gameplay/weapons/longshot/w_longshot.vmdl" );
		}

		public override void CreateViewModel()
		{
			base.CreateViewModel();

			if ( ViewModelEntity is ViewModel viewModel )
			{
				var aimConfig = new ViewModelAimConfig
				{
					AutoHide = true,
					Position = Vector3.Backward * 20f + Vector3.Left * 18f + Vector3.Up * 4f,
					Speed = 3f
				};

				viewModel.AimConfig = aimConfig;
			}
		}

		public override void Simulate( IClient owner )
		{
			if ( Game.IsClient && Input.Pressed( "run" ) )
			{
				if ( Prediction.FirstTime )
				{
					SetScoped( !IsScoped );
					PlaySound( "longshot.scope" );
				}
			}

			base.Simulate( owner );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			Game.SetRandomSeed( Time.Tick );

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"longshot.fire{Game.Random.Int(1, 2)}" );
			ShootBullet( 0f, 5f, Config.Damage, 16.0f );

			if ( AmmoClip == 0 )
				PlaySound( "pulserifle.empty" );

			TimeSincePrimaryAttack = 0f;
		}

		public override void PlayReloadSound()
		{
			PlaySound( "pulserifle.reload" );
			base.PlayReloadSound();
		}

		public override void SimulateAnimator( AnimationHelperWithLegs anim )
		{
			anim.HoldType = AnimationHelperWithLegs.HoldTypes.Rifle;
		}

		protected override ModelEntity GetEffectEntity()
		{
			return IsScoped ? this : EffectEntity;
		}

		protected override void OnDestroy()
		{
			if ( IsScoped )
			{
				SetScoped( false );
			}

			base.OnDestroy();
		}

		[Event.Client.BuildInput]
		protected new virtual void BuildInput()
		{
			if ( IsScoped )
			{
				Input.AnalogLook *= 0.2f;
			}
		}
	}
}
