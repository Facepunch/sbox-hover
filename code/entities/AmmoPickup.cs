using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public class AmmoPickup : ModelEntity, IHudEntity
	{
		public UI.EntityHudAnchor Hud { get; private set; }
		public UI.EntityHudIcon Icon { get; private set; }
		public Vector3 LocalCenter => CollisionBounds.Center;
		
		private RealTimeUntil TimeUntilAutoDelete { get; set; }
		private AmmoType AmmoType { get; set; }
		private int AmmoAmount { get; set; }

		public void SetAmmoType( AmmoType type, int amount )
		{
			AmmoType = type;
			AmmoAmount = amount;
		}
		
		public override void Spawn()
		{
			TimeUntilAutoDelete = 60f;
			Model = Cloud.Model( "pukes.smg_ammo_box" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			Tags.Add( "pickup" );
			
			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Hud = UI.EntityHud.Create( this );
			Hud.UpOffset = 20f;

			Icon = Hud.AddChild<UI.EntityHudIcon>( "ammo" );
			Icon.SetTexture( "ui/icons/ammo.png" );
			
			base.ClientSpawn();
		}
		
		public bool ShouldUpdateHud()
		{
			return true;
		}

		public void UpdateHudComponents()
		{
			var player = Game.LocalPawn as HoverPlayer;
			if ( !player.IsValid() ) return;
			
			var distance = player.Position.Distance( Position );
			var fadeInDistance = 400f;
			
			Icon.Style.Opacity = distance >= fadeInDistance ? distance.Remap( fadeInDistance, fadeInDistance * 1.25f, 1f, 0f ) : 1f;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is not HoverPlayer player ) return;
			if ( player.LifeState != LifeState.Alive ) return;
			if ( Game.IsClient ) return;

			var weapons = player.Children.OfType<Weapon>();
			var usesAmmoType = weapons.Any( weapon => weapon.Config.AmmoType == AmmoType );
			if ( !usesAmmoType ) return;
				
			player.GiveAmmo( AmmoType, AmmoAmount );
			Delete();
		}

		protected override void OnDestroy()
		{
			Hud?.Delete( true );
			Hud = null;
			
			base.OnDestroy();
		}

		[GameEvent.Tick.Server]
		private void ServerTick()
		{
			if ( TimeUntilAutoDelete )
			{
				Delete();
			}
		}
	}
}
