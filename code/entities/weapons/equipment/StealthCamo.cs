using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library]
	public class StealthCamoConfig : WeaponConfig
	{
		public override string Name => "Stealth";
		public override string Description => "Stealth Camouflage Ability";
		public override string Icon => "ui/equipment/stealth_camo.png";
		public override string ClassName => "hv_stealth_camo";
		public override List<Type> Upgrades => new()
		{
			typeof( StealthCamoUpgrade ),
			typeof( StealthCamoUpgrade ),
			typeof( StealthCamoUpgrade )
		};
		public override WeaponType Type => WeaponType.Equipment;
	}

	[Library( "hv_stealth_camo", Title = "Stealth" )]
	public partial class StealthCamo : Equipment
	{
		[Net] public RealTimeUntil NextReturnToStealth { get; set; }

		public override WeaponConfig Config => new StealthCamoConfig();
		public override InputButton? AbilityButton => InputButton.Flashlight;
		public override string AbilityBind => "iv_flashlight";
		public override bool IsPassive => true;

		public float EnergyDrain { get; set; } = 8f;

		private RealTimeUntil NextJammerCheck { get; set; }
		private Particles Effect { get; set; }

		public override DamageInfo OwnerTakeDamage( DamageInfo info )
		{
			if ( IsUsingAbility )
			{
				if ( info.Flags.HasFlag( DamageFlags.Shock ) )
					DisableAbility();
				else
					NextReturnToStealth = 1f;
			}

			return base.OwnerTakeDamage( info );
		}

		public override void OnAbilityUsed()
		{
			if ( IsServer )
			{
				using ( Prediction.Off() )
				{
					if ( IsUsingAbility )
						DisableAbility();
					else
						EnableAbility();
				}
			}

			base.OnAbilityUsed();
		}

		protected bool IsEnemyDisruptor( Disruptor jammer )
		{
			if ( Owner is not HoverPlayer player )
				return false;

			if ( jammer.Owner is not HoverPlayer other )
				return false;

			return player.Team == other.Team;
		}

		protected bool IsEnemyJammer( RadarJammer jammer )
		{
			if ( Owner is not HoverPlayer player )
				return false;

			if ( jammer.Owner is not HoverPlayer other )
				return false;

			return player.Team == other.Team;
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( Owner is not HoverPlayer player )
				return;

			if ( IsUsingAbility )
			{
				var energyDrain = EnergyDrain * Time.Delta;

				if ( player.IsRegeneratingEnergy )
				{
					energyDrain = (player.EnergyRegen + EnergyDrain) * Time.Delta;
				}

				player.Energy = Math.Max( player.Energy - energyDrain, 0f );

				if ( player.Energy <= 1f )
				{
					DisableAbility();
					return;
				}

				if ( NextJammerCheck )
				{
					var jammers = Entity.FindInSphere( Position, 1000f )
						.OfType<RadarJammer>();

					foreach ( var jammer in jammers )
					{
						if ( IsEnemyJammer( jammer ) && jammer.IsUsingAbility )
						{
							DisableAbility();
							return;
						}
					}

					var disruptors = Entity.FindInSphere( Position, 1000f )
						.OfType<Disruptor>()
						.Where( IsEnemyDisruptor );

					if ( disruptors.Any() )
					{
						DisableAbility();
						return;
					}

					NextJammerCheck = 1f;
				}

				if ( player.Controller is MoveController controller && controller.IsJetpacking )
				{
					NextReturnToStealth = 1f;
				}

				if ( player.ActiveChild is Weapon weapon && weapon.TimeSincePrimaryAttack <= 0.25f )
				{
					NextReturnToStealth = 1f;
				}

				if ( NextReturnToStealth )
					player.TargetAlpha = 0f;
				else
					player.TargetAlpha = 0.6f;
			}
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( Owner is HoverPlayer player && player.IsLocalPawn )
			{
				if ( player.TargetAlpha == 0f )
				{
					if ( Effect == null )
					{
						Effect = Particles.Create( "particles/player/cloaked.vpcf" );
					}
				}
				else
				{
					Effect?.Destroy();
					Effect = null;
				}
			}
		}

		protected override void OnDestroy()
		{
			if ( IsUsingAbility )
			{
				DisableAbility();
			}

			Effect?.Destroy();
			Effect = null;

			base.OnDestroy();
		}

		protected virtual void DisableAbility()
		{
			if ( Owner is not HoverPlayer player )
				return;

			player.TargetAlpha = 1f;
			player.PlaySound( "stealth.off" );

			IsUsingAbility = false;
		}

		protected virtual void EnableAbility()
		{
			if ( Owner is not HoverPlayer player )
				return;

			if ( player.Energy < 10f )
				return;

			player.TargetAlpha = 0f;
			player.PlaySound( "stealth.on" );

			IsUsingAbility = true;
		}
	}
}
