using Gamelib.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	[Library( "hv_deployable_blocker" )]
	[Hammer.AutoApplyMaterial("materials/editor/hv_outpost_volume.vmat")]
	[Hammer.Solid]
	public partial class DeployableBlocker : BaseTrigger
	{
		[Net, Property] public Team Team { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			EnableDrawing = false;
			Transmit = TransmitType.Always;
		}

		public override void StartTouch( Entity other )
		{
			if ( IsServer && other is Player player && player.Team == Team )
			{
				player.InDeployableBlocker = true;
			}

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( IsServer && other is Player player && player.Team == Team )
			{
				player.InDeployableBlocker = false;
			}

			base.EndTouch( other );
		}
	}
}
