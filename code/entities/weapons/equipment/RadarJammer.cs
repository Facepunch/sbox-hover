using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class RadarJammerConfig : WeaponConfig
	{
		public override string Name => "Jammer";
		public override string Description => "Invisible to Radar Sensors";
		public override string SecondaryDescription => "Reveals Stealth";
		public override string Icon => "ui/equipment/radar_jammer.png";
		public override string ClassName => "hv_radar_jammer";
	}

	[Library( "hv_radar_jammer", Title = "Jammer" )]
	public partial class RadarJammer : Equipment
	{
		public override WeaponConfig Config => new RadarJammerConfig();
		public override InputButton? AbilityButton => InputButton.Flashlight;
		public override string AbilityBind => "iv_flashlight";
		public override bool IsPassive => true;

		public float EnergyDrain { get; set; } = 4f;

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

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( Owner is not Player player )
				return;

			if ( player.Controller is not MoveController controller )
				return;

			if ( IsUsingAbility )
			{
				var energyDrain = EnergyDrain * Time.Delta;

				if ( controller.IsRegeneratingEnergy )
				{
					energyDrain = (controller.EnergyRegen + EnergyDrain) * Time.Delta;
				}

				controller.Energy = Math.Max( controller.Energy - energyDrain, 0f );
			}
		}

		protected override void OnDestroy()
		{
			if ( IsUsingAbility )
			{
				DisableAbility();
			}

			base.OnDestroy();
		}

		protected virtual void DisableAbility()
		{
			if ( Owner is not Player player )
				return;

			player.PlaySound( "stealth.off" );
			IsUsingAbility = false;
		}

		protected virtual void EnableAbility()
		{
			if ( Owner is not Player player )
				return;

			if ( player.Controller is not MoveController controller )
				return;

			if ( controller.Energy < 10f )
				return;

			player.ShouldHideOnRadar = 0f;
			player.PlaySound( "stealth.on" );

			IsUsingAbility = true;
		}
	}
}
