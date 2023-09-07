using Sandbox;
using Editor;
using System.ComponentModel.DataAnnotations;
using Sandbox.Utility;

namespace Facepunch.Hover
{
	[Library( "hv_generator" )]
	[EditorModel( "models/tempmodels/generator/generator_temp.vmdl", FixedBounds = true )]
	[Title( "Generator" )]
	[HammerEntity]
	public partial class GeneratorAsset : ModelEntity, IGameResettable, IUse, IHudEntity
	{
		public delegate void GeneratorEvent( GeneratorAsset generator );
		public static event GeneratorEvent OnGeneratorAttacked;
		public static event GeneratorEvent OnGeneratorRepaired;
		public static event GeneratorEvent OnGeneratorBroken;

		[Net] public RealTimeSince TimeSinceLastDamage { get; set; }
		[Net] public RealTimeUntil StartRegenTime { get; set; }
		[Net] public float MaxHealth { get; set; } = 6000f;
		[Net] public bool IsDestroyed { get; set; }
		public float RepairRate { get; set; } = 500f;
		public Vector3 LocalCenter => CollisionBounds.Center;

		[Net, Property] public Team Team { get; set; }

		public UI.EntityHudAnchor Hud { get; private set; }
		public UI.EntityHudIcon Icon { get; private set; }

		private UI.WorldHealthBar HealthBarLeft { get; set; }
		private UI.WorldHealthBar HealthBarRight { get; set; }
		private UI.WorldGeneratorHud GeneratorHud { get; set; }
		
		private RealTimeUntil KillRepairEffectTime { get; set; }
		private RealTimeUntil NextAttackedNotification { get; set; }
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
			SceneObject.Attributes.Set( "ScrollSpeed", new Vector2( 0f, 1f ) );
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
			if ( user is HoverPlayer player && player.Loadout.CanRepairGenerator )
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
			if ( Game.LocalPawn is not HoverPlayer player )
			{
				return;
			}

			var distance = player.Position.Distance( Position );

			if ( !IsDestroyed && TimeSinceLastDamage < 5f )
			{
				Icon.Style.Opacity = UIUtil.GetMinMaxDistanceAlpha( distance, 500f, 0f, float.PositiveInfinity, float.PositiveInfinity );
				Icon.SetTexture( "ui/icons/generator_attacked.png" );
				Icon.SetClass( "attacked", true );
			}
			else
			{
				Icon.Style.Opacity = UIUtil.GetMinMaxDistanceAlpha( distance, 1000f, 0f, 2000f, 3000f );
				Icon.SetTexture( IsDestroyed ? "ui/icons/generator_offline.png" : "ui/icons/generator.png" );
				Icon.SetClass( "attacked", false );
			}
				
			Icon.SetActive( player.Team == Team );
		}

		public override void Spawn()
		{
			SetModel( "models/tempmodels/generator/generator_temp.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			Transmit = TransmitType.Always;

			if ( Team == Team.Blue )
				RenderColor = Color.Blue;
			else
				RenderColor = Color.Red;

			Health = MaxHealth;

			PlayIdleSound();

			base.Spawn();
		}

		public virtual bool CanPlayerRepair( HoverPlayer player )
		{
			return (player.Team == Team && Health < MaxHealth);
		}

		public override void ClientSpawn()
		{
			HealthBarLeft = new UI.WorldHealthBar();
			HealthBarLeft.SetEntity( this, "health_left" );
			HealthBarLeft.MaximumValue = MaxHealth;
			HealthBarLeft.WorldScale = 2f;
			HealthBarLeft.ShowIcon = false;

			HealthBarRight = new UI.WorldHealthBar();
			HealthBarRight.SetEntity( this, "health_right" );
			HealthBarRight.MaximumValue = MaxHealth;
			HealthBarRight.WorldScale = 2f;
			HealthBarRight.ShowIcon = false;

			GeneratorHud = new UI.WorldGeneratorHud();
			GeneratorHud.SetEntity( this, "repair_hud" );
			GeneratorHud.WorldScale = 2f;

			Hud = UI.EntityHud.Create( this );

			Icon = Hud.AddChild<UI.EntityHudIcon>( "generator" );
			Icon.SetTexture( "ui/icons/generator.png" );

			base.ClientSpawn();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( info.Attacker is HoverPlayer player && player.Team == Team )
			{
				// Players cannot destroy their own team's generator.
				return;
			}

			if ( !info.HasTag( "blast" ) )
				info.Damage *= 0.4f;
			else
				info.Damage *= 1.2f;

			if ( !IsDestroyed && NextAttackedEffect )
			{
				Particles.Create( "particles/generator/generator_attacked/generator_attacked.vpcf", this );
				NextAttackedEffect = 0.5f;
			}

			if ( NextAttackedNotification )
			{
				NextAttackedNotification = 10f;
				OnGeneratorAttacked?.Invoke( this );
			}
			
			TimeSinceLastDamage = 0f;
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

		[GameEvent.Tick.Server]
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

		[GameEvent.Tick.Client]
		public virtual void ClientTick()
		{
			HealthBarLeft.SetValue( Health );
			HealthBarLeft.SetIsLow( Health < MaxHealth * 0.2f );
			HealthBarRight.SetValue( Health );
			HealthBarRight.SetIsLow( Health < MaxHealth * 0.2f );

			var damage = 1f - (Health / MaxHealth);
			SceneObject.Attributes.Set( "Damage", Easing.EaseIn( damage ) * 0.5f );
		}

		[ClientRpc]
		private void OnClientGeneratorRepaired()
		{
			Icon.SetTexture( "ui/icons/generator.png" );
			SceneObject.Attributes.Set( "ScrollSpeed", new Vector2( 0f, 1f ) );
			OnGeneratorRepaired?.Invoke( this );
		}

		[ClientRpc]
		private void OnClientGeneratorBroken()
		{
			Icon.SetTexture( "ui/icons/generator_offline.png" );
			SceneObject.Attributes.Set( "ScrollSpeed", new Vector2( 0f, 0f ) );
			OnGeneratorBroken?.Invoke( this );
		}
	}
}
