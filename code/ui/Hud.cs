
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace Facepunch.Hover
{
	[Library]
	public partial class Hud : HudEntity<RootPanel>
	{
		[ClientRpc]
		public static void AddKillFeed( Player attacker, Player victim, Weapon weapon )
		{
			ToastList.Instance.AddKillFeed( attacker, victim, weapon );
		}

		[ClientRpc]
		public static void AddKillFeed( Player victim )
		{
			ToastList.Instance.AddKillFeed( victim );
		}

		public static void ToastAll( string text, string icon = "" )
		{
			Toast( To.Everyone, text, icon );
		}

		public static void Toast( Player player, string text, string icon = "" )
		{
			Toast( To.Single( player ), text, icon );
		}

		[ClientRpc]
		public static void Toast( string text, string icon = "" )
		{
			ToastList.Instance.AddItem( text, Texture.Load( icon ) );
		}

		public Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<EntityHud>();
			RootPanel.AddChild<RoundInfo>();
			RootPanel.AddChild<Vitals>();
			RootPanel.AddChild<Ammo>();
			RootPanel.AddChild<Tokens>();
			RootPanel.AddChild<Speedometer>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<Nameplates>();
			RootPanel.AddChild<DamageIndicator>();
			RootPanel.AddChild<HitIndicator>();
			RootPanel.AddChild<WeaponList>();
			RootPanel.AddChild<Crosshair>();
			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<Scoreboard>();
			RootPanel.AddChild<RespawnScreen>();
			RootPanel.AddChild<AwardQueue>();
			RootPanel.AddChild<ToastList>();
		}
	}
}
