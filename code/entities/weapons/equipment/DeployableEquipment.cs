using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public abstract partial class DeployableEquipment<T> : Equipment, IDeployableEquipment where T : DeployableEntity, new()
	{
		public override string ViewModelPath => null;
		public override bool IsMelee => true;
		public virtual string Model => "";
		public virtual float DeployScale => 1f;
		public virtual float MinDistanceFromOthers => 1000f;
		public virtual int MaxDeployables { get; set; } = 1;

		[Net] public int Deployables { get; set; }

		private ModelEntity Ghost { get; set; }

		public override bool IsAvailable()
		{
			return Deployables > 0;
		}

		public override void OnDeployablePickedUp( DeployableEntity entity )
		{
			if ( entity is T )
			{
				Deployables = Math.Min( Deployables + 1, MaxDeployables );
			}
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
						var turret = new T();
						turret.Scale = DeployScale;
						turret.Deployer = player;
						turret.SetTeam( player.Team );
						turret.Position = position;
						turret.Rotation = rotation;

						OnDeploy( turret );

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
			if ( owner is Player && IsClient )
			{
				CreateGhostModel();
			}

			base.ActiveStart( owner );
		}

		public override void ActiveEnd( Entity owner, bool dropped )
		{
			if ( owner is Player && IsClient )
			{
				DestroyGhostModel();
			}

			base.ActiveEnd( owner, dropped );
		}

		private void CreateGhostModel()
		{
			Ghost?.Delete();
			Ghost = new ModelEntity();
			Ghost.SetModel( Model );
			Ghost.Scale = DeployScale;
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

		protected virtual void OnDeploy( T deployable ) { }

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

			var isNearOthers = false;
			var existing = All.OfType<T>().Where( v => v.Deployer == Owner );

			foreach ( var entity in existing )
			{
				if ( entity.Position.Distance( trace.EndPos ) <= MinDistanceFromOthers )
				{
					isNearOthers = true;
					break;
				}
			}

			if ( !isNearOthers && trace.Hit && trace.Entity.IsWorld && trace.Normal.Dot( Vector3.Up ) >= 0.8f && trace.EndPos.Distance( Owner.Position ) >= 60f )
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
