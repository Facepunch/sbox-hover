
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class WorldUpgradeHud : WorldPanel
	{
		public GeneratorDependency Entity { get; private set; }
		public SimpleIconBar IconBar { get; private set; }
		public int MaximumValue { get; set; }
		public new float WorldScale { get; set; } = 1f;
		public Panel Container { get; set; }
		public Label Name { get; set; }
		public Label Description { get; set; }
		public Label TokensLeft { get; set; }
		public Label UseLabel { get; set; }

		public WorldUpgradeHud()
		{
			StyleSheet.Load( "/ui/WorldUpgradeHud.scss" );
			Container = Add.Panel( "container" );
			UseLabel = Container.Add.Label( $"Hold [{Input.GetKeyWithBinding( "iv_use" )}] to Upgrade", "label" );
			Name = Container.Add.Label( "", "name" );
			Description = Container.Add.Label( "", "description" );
			IconBar = Container.AddChild<SimpleIconBar>( "bar" );
			TokensLeft = Container.Add.Label( "0", "tokens" );
		}

		public void SetEntity( GeneratorDependency entity )
		{
			Entity = entity;
			
			AddClass( entity.Team.GetHudClass() );
		}

		public void SetTokensLeft( int value )
		{
			var fraction = (float)value / (float)MaximumValue;
			var length = Length.Fraction( fraction );

			if ( IconBar.InnerBar.Style.Width != length )
			{
				IconBar.InnerBar.Style.Width = length;
				IconBar.InnerBar.Style.Dirty();
				IconBar.InnerBar.SetClass( "hidden", fraction < 0.05f );
			}

			TokensLeft.Text = $"{(MaximumValue - value):C0}";
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

			var nextUpgrade = Entity.GetNextUpgrade();

			if ( nextUpgrade == null )
			{
				SetClass( "hidden", true );
				return;
			}

			var cameraPosition = CurrentView.Position;
			var transform = Transform;
			var position = Entity.WorldSpaceBounds.Center;
			var direction = (cameraPosition - position).Normal;

			transform.Position = position + direction * 70f;

			var targetRotation = Rotation.LookAt( cameraPosition - Position );
			transform.Rotation = Rotation.Lerp( transform.Rotation, targetRotation, 0.4f );

			MaximumValue = nextUpgrade.TokenCost;
			Transform = transform;

			Description.Text = nextUpgrade.Description;
			Name.Text = nextUpgrade.Name;

			SetTokensLeft( Entity.UpgradeTokens );
			SetClass( "hidden", false );

			base.Tick();
		}
	}
}
