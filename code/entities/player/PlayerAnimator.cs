using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class PlayerAnimator : StandardPlayerAnimator
	{
		private float Skid { get; set; }

		public override void Simulate()
		{
			base.Simulate();

			if ( Velocity.Length > 90f )
			{
				if ( !HasTag( "skiing" ) && Input.Forward == 0f && Input.Left == 0f )
					Skid = Skid.LerpTo( 1f, Time.Delta * 5f );
				else
					Skid = Skid.LerpTo( 0f, Time.Delta * 5f );
			}
			else
			{
				Skid = Skid.LerpTo( 0f, Time.Delta * 5f );
			}

			SetAnimParameter( "skid", Skid );
		}
	}
}
