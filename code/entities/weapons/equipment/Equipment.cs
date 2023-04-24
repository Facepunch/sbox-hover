using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public abstract partial class Equipment : Weapon
	{
		public virtual string AbilityButton => null;

		[Net] public bool IsUsingAbility { get; protected set; }

		protected virtual void OnEquipmentGiven( HoverPlayer player ) { }
		protected virtual void OnEquipmentTaken( HoverPlayer player ) { }

		public virtual DamageInfo OwnerTakeDamage( DamageInfo info )
		{
			return info;
		}

		public virtual void OnAbilityUsed() { }

		public virtual void OnDeployablePickedUp( DeployableEntity entity ) { }

		public override void OnCarryStart( Entity carrier )
		{
			if ( carrier is HoverPlayer player )
			{
				OnEquipmentGiven( player );
			}

			base.OnCarryStart( carrier );
		}

		protected override void OnDestroy()
		{
			if ( Owner is HoverPlayer player )
			{
				OnEquipmentTaken( player );
			}

			base.OnDestroy();
		}
	}
}
