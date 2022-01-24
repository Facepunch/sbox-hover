
using Gamelib.UI;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Gamelib.Extensions;

namespace Facepunch.Hover
{
	public class WorldDeployableHud : WorldPanel
	{
		public DeployableEntity Entity { get; private set; }
		public SimpleIconBar IconBar { get; private set; }
		public new float WorldScale { get; set; } = 1f;
		public Panel Container { get; set; }

		public void SetEntity( DeployableEntity entity )
		{
			Entity = entity;
			
			AddClass( entity.Team.GetHudClass() );
		}

		public void SetProgress( float progress )
		{
			var length = Length.Fraction( progress );

			if ( IconBar.InnerBar.Style.Width != length )
			{
				IconBar.InnerBar.Style.Width = length;
				IconBar.InnerBar.Style.Dirty();
				IconBar.InnerBar.SetClass( "hidden", progress < 0.05f );
			}

			IconBar.SetClass( "hidden", progress == 0f );
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player )
				return;

			if ( IsDeleting ) return;

			if ( !Entity.IsValid() )
			{
				Delete();
				return;
			}

			if ( !Entity.IsUsable( player ) )
			{
				SetClass( "hidden", true );
				return;
			}

			var distance = player.Position.Distance( Entity.Position );
			Style.Opacity = UIUtility.GetMinMaxDistanceAlpha( distance, 100f, 500f );

			var cameraPosition = CurrentView.Position;
			var transform = Transform;
			var position = Entity.WorldSpaceBounds.Center;
			var direction = (cameraPosition - position).Normal;

			var pushDistance = Entity.WorldSpaceBounds.NearestPoint( cameraPosition ).Distance( position );
			transform.Position = position + direction * (pushDistance + 10f);

			var targetRotation = Rotation.LookAt( cameraPosition - Position );
			transform.Rotation = Rotation.Lerp( transform.Rotation, targetRotation, 0.4f );

			Transform = transform;

			SetProgress( Entity.PickupProgress );
			SetClass( "hidden", false );

			base.Tick();
		}
	}
}
