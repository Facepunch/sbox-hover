using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class GeneratorDependency : AnimatedEntity, IHudEntity, IGameResettable, IUse
	{
		public virtual float UpgradeTokensPerSecond => 50f;
		public virtual List<DependencyUpgrade> Upgrades => null;
		public virtual bool RequiresPower => true;
		public virtual string IconName => "";
		
		[Net, Change] public bool IsPowered { get; set; } = true;
		[Net] public int UpgradeTokens { get; private set; }
		[Net] public int NextUpgrade { get; private set; }

		public UI.WorldUpgradeHud UpgradeHud { get; private set; }
		public UI.EntityHudIcon NoPowerIcon { get; private set; }
		public UI.EntityHudAnchor Hud { get; private set; }

		[Net, Property, Change] public Team Team { get; set; }
		[Net] public Team DefaultTeam { get; set; }

		public RealTimeUntil NextStopUpgradeLoop { get; private set; }
		public UI.EntityHudIcon DependencyIcon { get; private set; }
		public Vector3 LocalCenter => CollisionBounds.Center;
		public Sound? UpgradeLoop { get; private set; }

		public virtual bool OnUse( Entity user )
		{
			if ( user is not HoverPlayer player )
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

		public virtual DependencyUpgrade GetNextUpgrade()
		{
			if ( Upgrades == null || Upgrades.Count == 0 )
				return null;

			if ( NextUpgrade >= Upgrades.Count )
				return null;

			return Upgrades[NextUpgrade];
		}

		public virtual void SetTeam( Team team )
		{
			var isPowered = true;
			var generator = All.OfType<GeneratorAsset>().Where( v => v.Team == team ).FirstOrDefault();

			if ( generator.IsValid() )
			{
				isPowered = !generator.IsDestroyed;
			}

			IsPowered = isPowered;

			if ( team == Team.Blue )
				RenderColor = Color.Blue;
			else if ( team == Team.Red )
				RenderColor = Color.Red;
			else if ( team == Team.None )
				RenderColor = Color.Yellow;

			Team = team;
		}

		public virtual bool IsUsable( Entity user )
		{
			if ( !IsPowered || DefaultTeam == Team.None )
				return false;

			if ( user is HoverPlayer player && player.Team == Team )
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
			SetTeam( DefaultTeam );
		}

		public virtual bool ShouldUpdateHud()
		{
			return true;
		}

		public virtual void UpdateHudComponents()
		{
			var distance = Game.LocalPawn.Position.Distance( Position ) - 1000f;
			var mapped = 1f - distance.Remap( 0f, 1000f, 0f, 1f );

			if ( NoPowerIcon.Style.Opacity != mapped )
			{
				NoPowerIcon.Style.Opacity = mapped;
			}

			if ( DependencyIcon != null && Game.LocalPawn is HoverPlayer player )
			{
				distance = player.Position.Distance( Position );

				DependencyIcon.Style.Opacity = UIUtil.GetMinMaxDistanceAlpha( distance, 1000f, 0f, 1250f, 1750f );
				DependencyIcon.SetActive( IsPowered && (Team == Team.None || player.Team == Team) );
			}
		}

		public override void Spawn()
		{
			if ( RequiresPower )
			{
				GeneratorAsset.OnGeneratorBroken += OnGeneratorBroken;
				GeneratorAsset.OnGeneratorRepaired += OnGeneratorRepaired;
			}

			DefaultTeam = Team;

			SetTeam( Team );

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Hud = UI.EntityHud.Create( this );

			NoPowerIcon = Hud.AddChild<UI.EntityHudIcon>( "power" );
			NoPowerIcon.SetTexture( "ui/icons/no-power.png" );
			NoPowerIcon.SetActive( !IsPowered );

			if ( !string.IsNullOrEmpty( IconName ) )
            {
				DependencyIcon = Hud.AddChild<UI.EntityHudIcon>( "dependency" );
				DependencyIcon.SetTexture( IconName );
				DependencyIcon.SetActive( IsPowered );
			} 

			if ( Upgrades != null && Upgrades.Count > 0 )
			{
				UpgradeHud = new UI.WorldUpgradeHud();
				UpgradeHud.SetEntity( this );
			}

			base.ClientSpawn();
		}

		protected override void OnDestroy()
		{
			if ( RequiresPower )
			{
				GeneratorAsset.OnGeneratorBroken -= OnGeneratorBroken;
				GeneratorAsset.OnGeneratorRepaired -= OnGeneratorRepaired;
			}

			if ( UpgradeLoop.HasValue )
			{
				UpgradeLoop.Value.Stop();
			}

			Hud?.Delete();

			base.OnDestroy();
		}

		[GameEvent.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( UpgradeLoop.HasValue && NextStopUpgradeLoop )
			{
				UpgradeLoop.Value.Stop();
				UpgradeLoop = null;
			}
		}

		protected virtual void OnTeamChanged( Team team )
		{

		}

		protected virtual void OnGeneratorRepaired( GeneratorAsset generator )
		{
			if ( generator.Team == Team )
			{
				PlaySound( "regen.start" );
				IsPowered = true;
			}
		}

		protected virtual void OnGeneratorBroken( GeneratorAsset generator )
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
			NoPowerIcon?.SetActive( !isPowered );
		}

		protected virtual void OnAddUpgrade( DependencyUpgrade upgrade )
		{
			PlaySound( "upgrade.complete" );
			upgrade.Apply( this );
		}

		protected virtual void OnFinishUpgrade( HoverPlayer player )
		{
			OnAddUpgrade( Upgrades[NextUpgrade] );
			UpgradeTokens = 0;
			NextUpgrade++;
		}
	}
}
