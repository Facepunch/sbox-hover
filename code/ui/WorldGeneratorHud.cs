
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.Hover
{
	public class WorldGeneratorHud : WorldPanel
	{
		public GeneratorAsset Entity { get; private set; }
		public string Attachment { get; set; }
		public new float WorldScale { get; set; } = 1f;
		public Panel Container { get; set; }
		public Label RegenLabel { get; set; }
		public Label EngineerLabel { get; set; }
		public BindingLabel BindingLabel { get; set; }

		public void SetEntity( GeneratorAsset entity, string attachment )
		{
			Entity = entity;
			Attachment = attachment;
			AddClass( entity.Team.GetHudClass() );
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

			if ( !Entity.CanPlayerRepair( player ) )
			{
				SetClass( "hidden", true );
				return;
			}

			var attachment = Entity.GetAttachment( Attachment );

			if ( attachment.HasValue )
			{
				Transform = attachment.Value.WithScale( WorldScale );
			}

			EngineerLabel.SetClass( "hidden", player.Loadout.CanRepairGenerator );
			BindingLabel.SetClass( "hidden", !player.Loadout.CanRepairGenerator );

			if ( Entity.StartRegenTime )
			{
				RegenLabel.SetClass( "hidden", true );
			}
			else
			{
				RegenLabel.SetClass( "hidden", false );
				RegenLabel.Text = $"Automatic Repair: {Entity.StartRegenTime.Relative.CeilToInt()}s";
			}

			var transform = Transform;
			transform.Rotation = Rotation.LookAt( CurrentView.Position - Position );
			Transform = transform;

			SetClass( "hidden", false );

			base.Tick();
		}
	}
}
