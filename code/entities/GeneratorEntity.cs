using Gamelib.UI;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	[Library( "hv_generator" )]
	[Hammer.EditorModel( "models/tempmodels/generator/generator_temp.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Generator", "Hover", "Defines a point where a team's generator spawns" )]
	public partial class GeneratorEntity : ModelEntity, IGameResettable, IUse, IHudEntity
	{
		public delegate void GeneratorEvent( GeneratorEntity generator );
		public static event GeneratorEvent OnGeneratorRepaired;
		public static event GeneratorEvent OnGeneratorBroken;

		[Net] public RealTimeUntil StartRegenTime { get; set; }
		[Net] public float MaxHealth { get; set; } = 6000f;
		[Net] public bool IsDestroyed { get; set; }
		public float RepairRate { get; set; } = 100f;
		public Vector3 LocalCenter => CollisionBounds.Center;

		[Net, Property] public Team Team { get; set; }

		public EntityHudAnchor Hud { get; private set; }
		public EntityHudIcon Icon { get; private set; }

		private WorldHealthBar HealthBarLeft { get; set; }
		private WorldHealthBar HealthBarRight { get; set; }
		private WorldGeneratorHud GeneratorHud { get; set; }

		private RealTimeUntil KillRepairEffectTime { get; set; }
		private RealTimeUntil NextAttackedEffect { get; set; }
		private Particles RepairEffect { get; set; }
		private bool IsRegenerating { get; set; }
		private Sound RepairSound { get; set; }
		private Sound IdleSound { get; set; }

		public void OnGameReset()
		{
			Health = MaxHealth;
			IsDestroyed = false;
			IsRegenerating = false;
			StopRepairSound();
			PlayIdleSound();
			OnClientGameReset();
		}

		[ClientRpc]
		public void OnClientGameReset()
		{
			SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 1f ) );
		}

		public void Repair()
		{
			Health = MaxHealth;
			IsDestroyed = false;
			IsRegenerating = false;
			StopRepairSound();
			PlayIdleSound();
			OnClientGeneratorRepaired();
			OnGeneratorRepaired?.Invoke( this );
		}

		public void StopRepairSound()
		{
			RepairSound.Stop();
		}

		public void PlayRepairSound()
		{
			RepairSound.Stop();
			RepairSound = PlaySound( "generator.repairloop" );
		}

		public void StopIdleSound()
		{
			IdleSound.Stop();
		}


		public void PlayIdleSound()
		{
			IdleSound.Stop();
			IdleSound = PlaySound( "generator.idleloop" );
		}

		public bool OnUse( Entity user )
		{
			if ( Health == MaxHealth )
			{
				return false;
			}

			if ( RepairEffect == null )
			{
				RepairEffect = Particles.Create( "particles/generator/generator_repair/generator_repair.vpcf", this );
				PlayRepairSound();
			}

			KillRepairEffectTime = 1f;
			Health += RepairRate * Time.Delta;

			if ( Health >= MaxHealth )
			{
				Repair();
				return false;
			}

			return true;
		}

		public bool IsUsable( Entity user )
		{
			if ( user is Player player )
			{
				return CanPlayerRepair( player );
			}

			return false;
		}

		public virtual bool ShouldUpdateHud()
		{
			return true;
		}

		public virtual void UpdateHudComponents()
		{
			if ( Local.Pawn is Player player )
			{
				var distance = player.Position.Distance( Position );

				Icon.Style.Opacity = UIUtility.GetMinMaxDistanceAlpha( distance, 1000f, 0f, 2000f, 3000f );
				Icon.SetActive( player.Team == Team );
			}
		}

		public override void Spawn()
		{
			SetModel( "models/tempmodels/generator/generator_temp.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

			Health = MaxHealth;

			PlayIdleSound();

			base.Spawn();
		}

		public virtual bool CanPlayerRepair( Player player )
		{
			return (player.Team == Team && Health < MaxHealth);
		}

		public override void ClientSpawn()
		{
			HealthBarLeft = new WorldHealthBar();
			HealthBarLeft.SetEntity( this, "health_left" );
			HealthBarLeft.MaximumValue = MaxHealth;
			HealthBarLeft.WorldScale = 2f;
			HealthBarLeft.ShowIcon = false;

			HealthBarRight = new WorldHealthBar();
			HealthBarRight.SetEntity( this, "health_right" );
			HealthBarRight.MaximumValue = MaxHealth;
			HealthBarRight.WorldScale = 2f;
			HealthBarRight.ShowIcon = false;

			GeneratorHud = new WorldGeneratorHud();
			GeneratorHud.SetEntity( this, "repair_hud" );
			GeneratorHud.WorldScale = 2f;

			Hud = EntityHud.Instance.Create( this );

			Icon = Hud.AddChild<EntityHudIcon>( "generator" );
			Icon.SetTexture( "ui/icons/generator.png" );

			base.ClientSpawn();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( info.Attacker is Player player && player.Team == Team )
			{
				// Players cannot destroy their own team's generator.
				return;
			}

			if ( !IsDestroyed && NextAttackedEffect )
			{
				Particles.Create( "particles/generator/generator_attacked/generator_attacked.vpcf", this );
				NextAttackedEffect = 0.5f;
			}

			StartRegenTime = 240f;
			IsRegenerating = false;

			base.TakeDamage( info );
		}

		public override void OnKilled()
		{
			if ( !IsDestroyed )
			{
				IsDestroyed = true;
				IsRegenerating = false;

				PlaySound( "barage.explode" );

				StopIdleSound();
				OnClientGeneratorBroken();
				OnGeneratorBroken?.Invoke( this );

				Particles.Create( "particles/generator/generator_destroy/generator_destroy.vpcf", this );

				PlaySound( "regen.energylow" );
			}
		}

		[Event.Tick.Server]
		public virtual void ServerTick()
		{
			if ( RepairEffect != null && KillRepairEffectTime )
			{
				RepairEffect?.Destroy();
				RepairEffect = null;
				StopRepairSound();
			}

			if ( IsDestroyed && StartRegenTime )
			{
				if ( !IsRegenerating )
				{
					IsRegenerating = true;
					PlaySound( "regen.start" );
				}

				if ( RepairEffect == null )
				{
					RepairEffect = Particles.Create( "particles/generator/generator_repair/generator_repair.vpcf", this );
					PlayRepairSound();
				}

				KillRepairEffectTime = 1f;
				Health += RepairRate * Time.Delta;

				if ( Health >= MaxHealth )
				{
					Repair();
				}
			}
		}

		[Event.Tick.Client]
		public virtual void ClientTick()
		{
			HealthBarLeft.SetValue( Health );
			HealthBarLeft.SetIsLow( Health < MaxHealth * 0.2f );
			HealthBarRight.SetValue( Health );
			HealthBarRight.SetIsLow( Health < MaxHealth * 0.2f );

			var damage = 1f - (Health / MaxHealth);
			SceneObject.SetValue( "Damage", Easing.EaseIn( damage ) * 0.5f );
		}

		[ClientRpc]
		private void OnClientGeneratorRepaired()
		{
			Icon.SetTexture( "ui/icons/generator.png" );
			SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 1f ) );
			OnGeneratorRepaired?.Invoke( this );
		}

		[ClientRpc]
		private void OnClientGeneratorBroken()
		{
			Icon.SetTexture( "ui/icons/generator_offline.png" );
			SceneObject.SetValue( "ScrollSpeed", new Vector2( 0f, 0f ) );
			OnGeneratorBroken?.Invoke( this );
		}
	}
}
