
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class WorldDeployableHud : WorldPanel
	{
		public DeployableEntity Entity { get; private set; }
		public SimpleIconBar IconBar { get; private set; }
		public new float WorldScale { get; set; } = 1f;
		public Panel Container { get; set; }
		public Label UseLabel { get; set; }

		public WorldDeployableHud()
		{
			StyleSheet.Load( "/ui/WorldDeployableHud.scss" );
			Container = Add.Panel( "container" );
			UseLabel = Container.Add.Label( $"Hold [{Input.GetKeyWithBinding( "iv_use" )}] to Pickup", "label" );
			IconBar = Container.AddChild<SimpleIconBar>( "bar" );
		}

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

			var cameraPosition = CurrentView.Position;
			var transform = Transform;
			var position = Entity.WorldSpaceBounds.Center;
			var direction = (cameraPosition - position).Normal;

			transform.Position = position + direction * (Entity.WorldSpaceBounds.Size.Length * 0.5f);

			var targetRotation = Rotation.LookAt( cameraPosition - Position );
			transform.Rotation = Rotation.Lerp( transform.Rotation, targetRotation, 0.4f );

			Transform = transform;

			SetProgress( Entity.PickupProgress );
			SetClass( "hidden", false );

			base.Tick();
		}
	}
}
