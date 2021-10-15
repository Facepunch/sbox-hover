using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public abstract partial class Equipment : Weapon
	{
		public virtual InputButton? AbilityButton => null;
		public virtual string AbilityBind => null;

		[Net] public bool IsUsingAbility { get; protected set; }

		protected virtual void OnEquipmentGiven( Player player ) { }
		protected virtual void OnEquipmentTaken( Player player ) { }

		public virtual DamageInfo OwnerTakeDamage( DamageInfo info )
		{
			return info;
		}

		public virtual void OnAbilityUsed() { }

		public override void OnCarryStart( Entity carrier )
		{
			if ( carrier is Player player )
			{
				OnEquipmentGiven( player );
			}

			base.OnCarryStart( carrier );
		}

		protected override void OnDestroy()
		{
			if ( Owner is Player player )
			{
				OnEquipmentTaken( player );
			}

			base.OnDestroy();
		}
	}
}
