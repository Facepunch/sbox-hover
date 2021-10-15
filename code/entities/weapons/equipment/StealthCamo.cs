using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public class StealthCamoConfig : WeaponConfig
	{
		public override string Name => "Stealth";
		public override string Description => "Stealth Camouflage Ability";
		public override string Icon => "ui/equipment/stealth_camo.png";
		public override string ClassName => "hv_stealth_camo";
	}

	[Library( "hv_stealth_camo", Title = "Stealth" )]
	public partial class StealthCamo : Equipment
	{
		public override WeaponConfig Config => new StealthCamoConfig();
		public override InputButton? AbilityButton => InputButton.Flashlight;
		public override string AbilityBind => "iv_flashlight";
		public override bool IsPassive => true;

		private RealTimeUntil NextReturnToStealth { get; set; }

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
				var energyDrain = (controller.EnergyRegen + 8f) * Time.Delta;
				controller.Energy = Math.Max( controller.Energy - energyDrain, 0f );

				if ( controller.Energy <= 1f )
				{
					DisableAbility();
					return;
				}

				if ( controller.IsJetpacking )
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

			player.TargetAlpha = 1f;
			player.PlaySound( "stealth.off" );

			IsUsingAbility = false;
		}

		protected virtual void EnableAbility()
		{
			if ( Owner is not Player player )
				return;

			player.TargetAlpha = 0f;
			player.PlaySound( "stealth.on" );

			IsUsingAbility = true;
		}
	}
}
