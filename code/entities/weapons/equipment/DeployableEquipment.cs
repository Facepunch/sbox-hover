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
		public virtual string ModelName => "";
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
			var existing = All.OfType<T>().Where( v => v.Deployer == Owner );

			Deployables = Math.Max( MaxDeployables - existing.Count(), 0 );

			base.Restock();
		}

		public override void Simulate( Client client )
		{
			if ( Ghost.IsValid() && Owner is Player player )
			{
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

			base.Simulate( client );
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
			Ghost.SetModel( ModelName );
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

		protected bool GetPlacePosition( out Vector3 position, out Rotation rotation )
		{
			var trace = Trace.Ray( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * 150f )
				.Ignore( this )
				.Ignore( Owner )
				.Run();

			var isNearOthers = false;
			var existing = All.OfType<T>().Where( v => v.Deployer == Owner );

			foreach ( var entity in existing )
			{
				if ( entity.Position.Distance( trace.EndPosition ) <= MinDistanceFromOthers )
				{
					isNearOthers = true;
					break;
				}
			}

			var inDeploymentBlocker = false;

			if ( Owner is Player player && player.InDeployableBlocker )
			{
				inDeploymentBlocker = true;
			}

			if ( !inDeploymentBlocker && !isNearOthers && trace.Hit && trace.Entity.IsWorld
				&& trace.Normal.Dot( Vector3.Up ) >= 0.8f && trace.EndPosition.Distance( Owner.Position ) >= 60f )
			{
				var lookAt = Vector3.Cross( trace.Normal, Owner.EyeRotation.Right );
				position = trace.EndPosition;
				rotation = Rotation.LookAt( lookAt, trace.Normal );

				return true;
			}
			else if ( trace.Normal.Dot( Vector3.Up ) >= 0.8f )
			{
				var lookAt = Vector3.Cross( Vector3.Up, Owner.EyeRotation.Right );
				position = trace.EndPosition;
				rotation = Rotation.LookAt( lookAt, trace.Normal );

				return false;
			}
			else
			{
				var lookAt = Vector3.Cross( Vector3.Up, Owner.EyeRotation.Right );
				position = trace.EndPosition;
				rotation = Rotation.LookAt( lookAt, Vector3.Up );

				return false;
			}
		}
	}
}
