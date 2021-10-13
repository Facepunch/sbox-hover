using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public abstract partial class Equipment : Weapon
	{
		protected virtual void OnEquipmentGiven( Player player ) { }
		protected virtual void OnEquipmentTaken( Player player ) { }

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
