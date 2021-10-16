using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class GeneratorDependency : AnimEntity, IHudEntity, IGameResettable, IUse
	{
		public virtual float UpgradeTokensPerSecond => 50f;
		public virtual List<DependencyUpgrade> Upgrades => null;
		
		[Net, Change] public bool IsPowered { get; set; } = true;
		[Net] public int UpgradeTokens { get; private set; }
		[Net] public int NextUpgrade { get; private set; }

		public WorldUpgradeHud UpgradeHud { get; private set; }
		public EntityHudIcon NoPowerIcon { get; private set; }
		public EntityHudAnchor Hud { get; private set; }

		[Net, Property] public Team Team { get; set; }

		public RealTimeUntil NextStopUpgradeLoop { get; private set; }
		public Vector3 LocalCenter => CollisionBounds.Center;
		public Sound? UpgradeLoop { get; private set; }

		public bool OnUse( Entity user )
		{
			if ( user is not Player player )
				return false;

			var nextUpgrade = GetNextUpgrade();

			if ( nextUpgrade == null )
				return false;

			var tokensToContribute = (UpgradeTokensPerSecond * Time.Delta).CeilToInt();
			var targetTokens = nextUpgrade.TokenCost;

			if ( UpgradeTokens + tokensToContribute > targetTokens )
			{
				tokensToContribute = (UpgradeTokens + tokensToContribute) - targetTokens;
			}

			if ( player.HasTokens( tokensToContribute ) )
			{
				if ( UpgradeLoop == null )
				{
					UpgradeLoop = PlaySound( "upgrade.loop" );
				}

				NextStopUpgradeLoop = 0.5f;

				player.TakeTokens( tokensToContribute );
				UpgradeTokens += tokensToContribute;

				if ( UpgradeTokens >= targetTokens )
				{
					OnFinishUpgrade( player );
					return false;
				}

				return true;
			}

			PlaySound( "player_use_fail" );

			return false;
		}

		public DependencyUpgrade GetNextUpgrade()
		{
			if ( Upgrades == null || Upgrades.Count == 0 )
				return null;

			if ( NextUpgrade >= Upgrades.Count )
				return null;

			return Upgrades[NextUpgrade];
		}

		public bool IsUsable( Entity user )
		{
			if ( !IsPowered )
				return false;

			if ( user is Player player && player.Team == Team )
			{
				if ( player.Loadout.CanUpgradeDependencies )
				{
					return GetNextUpgrade() != null;
				}
			}

			return false;
		}

		public virtual void OnGameReset()
		{
			UpgradeTokens = 0;
			NextUpgrade = 0;
			IsPowered = true;
		}

		public virtual bool ShouldUpdateHud()
		{
			return true;
		}

		public virtual void UpdateHudComponents()
		{
			var distance = Local.Pawn.Position.Distance( Position ) - 1000f;
			var mapped = 1f - distance.Remap( 0f, 1000f, 0f, 1f );

			if ( Hud.Style.Opacity != mapped )
			{
				Hud.Style.Opacity = mapped;
				Hud.Style.Dirty();
			}
		}

		public override void Spawn()
		{
			GeneratorEntity.OnGeneratorBroken += OnGeneratorBroken;
			GeneratorEntity.OnGeneratorRepaired += OnGeneratorRepaired;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Hud = EntityHud.Instance.Create( this );
			Hud.SetActive( false );

			NoPowerIcon = Hud.AddChild<EntityHudIcon>( "power" );
			NoPowerIcon.SetTexture( "ui/icons/no-power.png" );

			if ( Upgrades != null && Upgrades.Count > 0 )
			{
				UpgradeHud = new WorldUpgradeHud();
				UpgradeHud.SetEntity( this );
			}

			base.ClientSpawn();
		}

		protected override void OnDestroy()
		{
			GeneratorEntity.OnGeneratorBroken -= OnGeneratorBroken;
			GeneratorEntity.OnGeneratorRepaired -= OnGeneratorRepaired;

			if ( UpgradeLoop.HasValue )
			{
				UpgradeLoop.Value.Stop();
			}

			base.OnDestroy();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( UpgradeLoop.HasValue && NextStopUpgradeLoop )
			{
				UpgradeLoop.Value.Stop();
				UpgradeLoop = null;
			}
		}

		protected virtual void OnGeneratorRepaired( GeneratorEntity generator )
		{
			if ( generator.Team == Team )
			{
				PlaySound( "regen.start" );
				IsPowered = true;
			}
		}

		protected virtual void OnGeneratorBroken( GeneratorEntity generator )
		{
			if ( generator.Team == Team )
			{
				// TODO: Replace this with a power down particle?
				Particles.Create( "particles/generator/generator_attacked/generator_attacked.vpcf", this );
				PlaySound( "regen.energylow" );
				IsPowered = false;
			}
		}

		protected virtual void OnIsPoweredChanged( bool isPowered )
		{
			Hud.SetActive( !isPowered );
		}

		protected virtual void OnAddUpgrade( DependencyUpgrade upgrade )
		{
			PlaySound( "upgrade.complete" );
			upgrade.Apply( this );
		}

		protected virtual void OnFinishUpgrade( Player player )
		{
			OnAddUpgrade( Upgrades[NextUpgrade] );
			UpgradeTokens = 0;
			NextUpgrade++;
		}
	}
}
