using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public class DeployableTurretConfig : WeaponConfig
	{
		public override string Name => "Turret";
		public override string Description => "Deployable Light Turret";
		public override string Icon => "ui/equipment/deployable_turret.png";
		public override string ClassName => "hv_deployable_turret";
	}

	[Library( "hv_deployable_turret", Title = "Turret" )]
	public partial class DeployableTurret : Equipment
	{
		public override WeaponConfig Config => new DeployableTurretConfig();
		public override string ViewModelPath => null;
		public override bool IsMelee => true;
		public virtual int MaxDeployables => 3;

		[Net] public int Deployables { get; set; }

		private ModelEntity Ghost { get; set; }

		public override bool IsAvailable()
		{
			return Deployables > 0;
		}

		public override void Restock()
		{
			var existing = All.OfType<LightTurret>().Where( v => v.Deployer == Owner );

			Deployables = Math.Max( MaxDeployables - existing.Count(), 0 );

			base.Restock();
		}

		public override void AttackPrimary()
		{
			if ( Owner is not Player player ) return;

			if ( Deployables == 0 )
			{
				PlaySound( "blaster.empty" );
				return;
			}

			var canDeploy = GetPlacePosition( out var position, out var rotation );

			if ( canDeploy )
			{
				if ( IsServer )
				{
					using ( Prediction.Off() )
					{
						var turret = new LightTurret();
						turret.Deployer = player;
						turret.SetTeam( player.Team );
						turret.Position = position;
						turret.Rotation = rotation;

						Deployables--;

						if ( Deployables == 0 )
						{
							PlaySound( "blaster.empty" );
							return;
						}
					}
				}
			}
			else
			{
				PlaySound( "blaster.empty" );
			}
		}

		public override void ActiveStart( Entity owner )
		{
			if ( owner is Player player && IsClient )
			{
				CreateGhostModel();
			}

			base.ActiveStart( owner );
		}

		public override void ActiveEnd( Entity owner, bool dropped )
		{
			if ( owner is Player player && IsClient )
			{
				DestroyGhostModel();
			}

			base.ActiveEnd( owner, dropped );
		}

		private void CreateGhostModel()
		{
			Ghost?.Delete();
			Ghost = new ModelEntity();
			Ghost.SetModel( "models/deploy_turret/deploy_turret.vmdl" );
		}

		private void DestroyGhostModel()
		{
			Ghost?.Delete();
			Ghost = null;
		}

		protected override void OnDestroy()
		{
			DestroyGhostModel();

			base.OnDestroy();
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( !Ghost.IsValid() ) return;

			if ( Owner is not Player player ) return;

			var canDeploy = GetPlacePosition( out var position, out var rotation );

			if ( canDeploy )
			{
				Ghost.RenderColor = player.Team.GetColor().WithAlpha( 0.9f );
			}
			else
			{
				Ghost.RenderColor = player.Team.GetColor().WithAlpha( 0.5f );
			}

			Ghost.Position = position;
			Ghost.Rotation = rotation;
		}

		protected bool GetPlacePosition( out Vector3 position, out Rotation rotation )
		{
			var trace = Trace.Ray( Owner.EyePos, Owner.EyePos + Owner.EyeRot.Forward * 150f )
				.Ignore( this )
				.Ignore( Owner )
				.Run();

			if ( trace.Hit && trace.Entity.IsWorld && trace.Normal.Dot( Vector3.Up ) >= 0.8f && trace.EndPos.Distance( Owner.Position ) >= 60f )
			{
				var lookAt = Vector3.Cross( trace.Normal, Owner.EyeRot.Right );
				position = trace.EndPos;
				rotation = Rotation.LookAt( lookAt, trace.Normal );

				return true;
			}
			else
			{
				var lookAt = Vector3.Cross( Vector3.Up, Owner.EyeRot.Right );
				position = trace.EndPos;
				rotation = Rotation.LookAt( lookAt, trace.Normal );

				return false;
			}
		}
	}
}
