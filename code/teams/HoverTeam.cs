using Sandbox;

namespace Facepunch.Hover
{
    public class HoverTeam : BaseTeam
	{
		private Radar _radarHud;

		public override void SupplyLoadout( Player player  )
		{
			player.ClearAmmo();
			player.Inventory.DeleteContents();
			player.Inventory.Add( new Pistol(), true );
			player.Inventory.Add( new SMG(), true );
			player.GiveAmmo( AmmoType.Pistol, 120 );
		}

		public override void OnStart( Player player )
		{
			player.ClearAmmo();
			player.Inventory.DeleteContents();

			player.SetModel( "models/citizen/citizen.vmdl" );

			if ( Host.IsServer )
			{
				player.RemoveClothing();
				player.AttachClothing( "models/citizen_clothes/trousers/trousers.lab.vmdl" );
				player.AttachClothing( "models/citizen_clothes/jacket/labcoat.vmdl" );
				player.AttachClothing( "models/citizen_clothes/shoes/shoes.workboots.vmdl" );
				player.AttachClothing( "models/citizen_clothes/hat/hat_securityhelmet.vmdl" );
			}

			player.RemoveAllDecals();

			player.EnableAllCollisions = true;
			player.EnableDrawing = true;
			player.EnableHideInFirstPerson = true;
			player.EnableShadowInFirstPerson = true;

			player.Controller = new MoveController();
			player.Camera = new FirstPersonCamera();
		}

		public override void OnJoin( Player player  )
		{
			var client = player.GetClientOwner();

			if ( Host.IsClient && player.IsLocalPawn )
			{
				_radarHud = Local.Hud.AddChild<Radar>();
			}

			base.OnJoin( player );
		}

		public override void OnPlayerKilled( Player player )
		{
			player.GlowActive = false;
		}

		public override void OnLeave( Player player  )
		{
			var client = player.GetClientOwner();

			if ( player.IsLocalPawn )
			{
				if ( _radarHud != null )
				{
					_radarHud.Delete( true );
					_radarHud = null;
				}
			}

			base.OnLeave( player );
		}
	}
}
