using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class AssaultLoadout : BaseLoadout
	{
		public override void SupplyLoadout()
		{
			Entity.ClearAmmo();
			Entity.Inventory.DeleteContents();
			Entity.Inventory.Add( new Pistol(), true );
			Entity.Inventory.Add( new SMG(), true );
			Entity.GiveAmmo( AmmoType.Pistol, 120 );
		}

		public override void Setup()
		{
			Entity.ClearAmmo();
			Entity.Inventory.DeleteContents();
			Entity.SetModel( "models/citizen/citizen.vmdl" );

			Entity.RemoveClothing();
			Entity.AttachClothing( "models/citizen_clothes/trousers/trousers.lab.vmdl" );
			Entity.AttachClothing( "models/citizen_clothes/jacket/labcoat.vmdl" );
			Entity.AttachClothing( "models/citizen_clothes/shoes/shoes.workboots.vmdl" );
			Entity.AttachClothing( "models/citizen_clothes/hat/hat_securityhelmet.vmdl" );

			Entity.RemoveAllDecals();

			Entity.EnableAllCollisions = true;
			Entity.EnableDrawing = true;
			Entity.EnableHideInFirstPerson = true;
			Entity.EnableShadowInFirstPerson = true;

			Entity.Controller = new MoveController();
			Entity.Camera = new FirstPersonCamera();
		}
	}
}
