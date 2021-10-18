
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class WorldStationHud : WorldPanel
	{
		public StationEntity Entity { get; private set; }
		public SimpleIconBar IconBar { get; private set; }
		public float MaximumValue { get; set; }
		public string Attachment { get; set; }
		public new float WorldScale { get; set; } = 1f;
		public Label UseLabel { get; set; }

		public WorldStationHud()
		{
			StyleSheet.Load( "/ui/WorldStationHud.scss" );
			UseLabel = Add.Label( $"Press [{Input.GetKeyWithBinding( "iv_use" )}] to Open Station", "label" );
			IconBar = AddChild<SimpleIconBar>( "restock" );
		}

		public void SetEntity( StationEntity entity, string attachment )
		{
			Entity = entity;
			Attachment = attachment;
		}

		public void SetRestockTime( float value )
		{
			var fraction = Length.Fraction( 1f - ( value / 30f ) );

			if ( IconBar.InnerBar.Style.Width != fraction )
			{
				IconBar.InnerBar.Style.Width = fraction;
				IconBar.InnerBar.Style.Dirty();
			}
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

			if ( !Entity.CanPlayerUse( player ) )
			{
				SetClass( "hidden", true );
				return;
			}

			SetClass( Team.Red.GetHudClass(), Entity.Team == Team.Red );
			SetClass( Team.Blue.GetHudClass(), Entity.Team == Team.Blue );
			SetClass( Team.None.GetHudClass(), Entity.Team == Team.None );

			var attachment = Entity.GetAttachment( Attachment );

			if ( attachment.HasValue )
			{
				Transform = attachment.Value.WithScale( WorldScale );
			}

			var targetRotation = Rotation.LookAt( CurrentView.Position - Position );
			var transform = Transform;

			transform.Rotation = Rotation.Lerp( transform.Rotation, targetRotation, 0.4f );

			Transform = transform;

			SetRestockTime( player.NextStationRestock );
			SetClass( "hidden", false );

			base.Tick();
		}
	}
}
