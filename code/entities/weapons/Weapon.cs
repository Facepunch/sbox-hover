using Gamelib.Extensions;
using Gamelib.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class Weapon : BaseWeapon
	{
		public virtual AmmoType AmmoType => AmmoType.Pistol;
		public virtual string MuzzleAttachment => "muzzle";
		public virtual string MuzzleFlashEffect => "particles/pistol_muzzleflash.vpcf";
		public virtual List<string> FlybySounds => new()
		{
			"flyby.rifleclose1",
			"flyby.rifleclose2",
			"flyby.rifleclose3",
			"flyby.rifleclose4"
		};
		public virtual string ImpactEffect => null;
		public virtual int ClipSize => 16;
		public virtual float ReloadTime => 3.0f;
		public virtual bool IsMelee => false;
		public virtual int Slot => 0;
		public virtual Texture Icon => null;
		public virtual float DamageFalloffStart => 0f;
		public virtual float DamageFalloffEnd => 0f;
		public virtual float BulletRange => 20000f;
		public virtual string TracerEffect => null;
		public virtual string WeaponName => "Weapon";
		public virtual bool ReloadAnimation => true;
		public virtual bool UnlimitedAmmo => false;
		public virtual bool CanMeleeAttack => false;
		public virtual float MeleeDamage => 100f;
		public virtual float MeleeRange => 200f;
		public virtual float MeleeRate => 1f;
		public virtual float ChargeAttackDuration => 2f;
		public virtual DamageFlags DamageType => DamageFlags.Bullet;
		public virtual int BaseDamage => 10;
		public virtual int HoldType => 1;
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		[Net, Predicted]
		public int AmmoClip { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceReload { get; set; }

		[Net, Predicted]
		public bool IsReloading { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceDeployed { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceChargeAttack { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceMeleeAttack { get; set; }

		public float ChargeAttackEndTime { get; private set; }
		public AnimEntity AnimationOwner => Owner as AnimEntity;

		public int AvailableAmmo()
		{
			if ( Owner is not Player owner ) return 0;
			return owner.AmmoCount( AmmoType );
		}

		public float GetDamageFalloff( float distance, float damage )
		{
			if ( DamageFalloffEnd > 0f )
			{
				if ( DamageFalloffStart > 0f )
				{
					if ( distance < DamageFalloffStart )
					{
						return damage;
					}

					var falloffRange = DamageFalloffEnd - DamageFalloffStart;
					var difference = (distance - DamageFalloffStart);

					return Math.Max( damage - (damage / falloffRange) * difference, 0f );
				}

				return Math.Max( damage - (damage / DamageFalloffEnd) * distance, 0f );
			}

			return damage;
		}

		public virtual void OnMeleeAttack()
		{
			ViewModelEntity?.SetAnimBool( "melee", true );
			TimeSinceMeleeAttack = 0f;
			MeleeStrike( MeleeDamage, 2f );
			PlaySound( "player.melee" );
		}

		public override bool CanReload()
		{
			if ( CanMeleeAttack && TimeSinceMeleeAttack < (1 / MeleeRate) )
			{
				return false;
			}

			return base.CanReload();
		}

		public override void ActiveStart( Entity owner )
		{
			base.ActiveStart( owner );

			TimeSinceDeployed = 0f;
		}

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		}

		public override void Reload()
		{
			if ( IsMelee || IsReloading )
				return;

			if ( AmmoClip >= ClipSize )
				return;

			TimeSinceReload = 0f;

			if ( Owner is Player player )
			{
				if ( !UnlimitedAmmo )
				{
					if ( player.AmmoCount( AmmoType ) <= 0 )
						return;
				}
			}

			IsReloading = true;

			if ( ReloadAnimation )
			{
				AnimationOwner.SetAnimBool( "b_reload", true );
			}

			PlayReloadSound();
			DoClientReload();
		}

		public override void Simulate( Client owner )
		{
			if ( owner.Pawn is Player player )
			{
				if ( owner.Pawn.LifeState == LifeState.Alive )
				{
					if ( ChargeAttackEndTime > 0f && Time.Now >= ChargeAttackEndTime )
					{
						OnChargeAttackFinish();
						ChargeAttackEndTime = 0f;
					}
				}
				else
				{
					ChargeAttackEndTime = 0f;
				}
			}

			if ( Input.Down( InputButton.Flashlight ) )
			{
				if ( CanMeleeAttack && TimeSinceMeleeAttack > (1 / MeleeRate) )
				{
					IsReloading = false;
					OnMeleeAttack();
					return;
				}
			}

			if ( !IsReloading )
			{
				base.Simulate( owner );
			}

			if ( IsReloading && TimeSinceReload > ReloadTime )
			{
				OnReloadFinish();
			}
		}

		public override bool CanPrimaryAttack()
		{
			if ( ChargeAttackEndTime > 0f && Time.Now < ChargeAttackEndTime )
				return false;

			return base.CanPrimaryAttack();
		}

		public override bool CanSecondaryAttack()
		{
			if ( ChargeAttackEndTime > 0f && Time.Now < ChargeAttackEndTime )
				return false;

			return base.CanSecondaryAttack();
		}

		public virtual void StartChargeAttack()
		{
			ChargeAttackEndTime = Time.Now + ChargeAttackDuration;
		}

		public virtual void OnChargeAttackFinish() { }

		public virtual void OnReloadFinish()
		{
			IsReloading = false;

			if ( Owner is Player player )
			{
				if ( !UnlimitedAmmo )
				{
					var ammo = player.TakeAmmo( AmmoType, ClipSize - AmmoClip );

					if ( ammo == 0 )
						return;

					AmmoClip += ammo;
				}
				else
				{
					AmmoClip = ClipSize;
				}
			}
		}

		public virtual void PlayReloadSound()
		{

		}

		[ClientRpc]
		public virtual void DoClientReload()
		{
			if ( ReloadAnimation )
			{
				ViewModelEntity?.SetAnimBool( "reload", true );
			}
		}

		public override void AttackPrimary()
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			ShootEffects();
			ShootBullet( 0.05f, 1.5f, BaseDamage, 3.0f );
		}

		public virtual void MeleeStrike( float damage, float force )
		{
			var forward = Owner.EyeRot.Forward;
			forward = forward.Normal;

			foreach ( var trace in TraceBullet( Owner.EyePos, Owner.EyePos + forward * MeleeRange, 40f ) )
			{
				if ( !trace.Entity.IsValid() ) continue;
				if ( !IsServer ) continue;

				using ( Prediction.Off() )
				{
					var damageInfo = new DamageInfo()
						.WithPosition( trace.EndPos )
						.WithFlag( DamageFlags.Blunt )
						.WithForce( forward * 100f * force )
						.UsingTraceResult( trace )
						.WithAttacker( Owner )
						.WithWeapon( this );

					damageInfo.Damage = damage;

					trace.Entity.TakeDamage( damageInfo );
				}
			}
		}

		public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
		{
			var forward = Owner.EyeRot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			foreach ( var trace in TraceBullet( Owner.EyePos, Owner.EyePos + forward * BulletRange, bulletSize ) )
			{
				if ( string.IsNullOrEmpty( ImpactEffect ) )
				{
					trace.Surface.DoBulletImpact( trace );
				}

				var fullEndPos = trace.EndPos + trace.Direction * bulletSize;

				if ( !string.IsNullOrEmpty( TracerEffect ) )
				{
					var muzzle = EffectEntity?.GetAttachment( MuzzleAttachment );
					var tracer = Particles.Create( TracerEffect );
					tracer.SetPosition( 0, muzzle.HasValue ? muzzle.Value.Position : trace.StartPos );
					tracer.SetPosition( 1, fullEndPos );
				}

				if ( !string.IsNullOrEmpty( ImpactEffect ) )
				{
					var impact = Particles.Create( ImpactEffect, fullEndPos );
					impact.SetForward( 0, trace.Normal );
				}

				if ( !IsServer )
					continue;

				WeaponUtil.PlayFlybySounds( Owner, trace.Entity, trace.StartPos, trace.EndPos, bulletSize * 2f, bulletSize * 50f, FlybySounds );

				if ( trace.Entity.IsValid() )
				{
					using ( Prediction.Off() )
					{
						var damageInfo = new DamageInfo()
							.WithPosition( trace.EndPos )
							.WithFlag( DamageType )
							.WithForce( forward * 100f * force )
							.UsingTraceResult( trace )
							.WithAttacker( Owner )
							.WithWeapon( this );

						damageInfo.Damage = GetDamageFalloff( trace.Distance, damage );

						trace.Entity.TakeDamage( damageInfo );
					}
				}
			}
		}

		public bool TakeAmmo( int amount )
		{
			if ( AmmoClip < amount )
				return false;

			AmmoClip -= amount;
			return true;
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new ViewModel
			{
				Position = Position,
				Owner = Owner,
				EnableViewmodelRendering = true
			};

			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void CreateHudElements()
		{
			if ( Local.Hud == null ) return;

			CrosshairPanel = new Crosshair
			{
				Parent = Local.Hud
			};

			CrosshairPanel.AddClass( ClassInfo.Name );
		}

		public bool IsUsable()
		{
			if ( IsMelee || ClipSize == 0 || AmmoClip > 0 )
			{
				return true;
			}

			return AvailableAmmo() > 0;
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			if ( !IsMelee && !string.IsNullOrEmpty( MuzzleFlashEffect ) )
			{
				Particles.Create( MuzzleFlashEffect, EffectEntity, "muzzle" );
			}

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin();
			}

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "fire" );
		}

		protected void DealDamage( Entity target, Vector3 position, Vector3 force )
		{
			DealDamage( target, position, force, BaseDamage );
		}

		protected void DealDamage( Entity target, Vector3 position, Vector3 force, float damage )
		{
			var damageInfo = new DamageInfo()
				.WithAttacker( Owner )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( force )
				.WithFlag( DamageType );

			damageInfo.Damage = damage;

			target.TakeDamage( damageInfo );
		}
	}
}
