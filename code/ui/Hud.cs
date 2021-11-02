﻿
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
		public static void AddKillFeed( Player attacker, Player victim, Entity weapon )
		{
			ToastList.Instance.AddKillFeed( attacker, victim, weapon );
		}

		[ClientRpc]
		public static void AddKillFeed( Entity attacker, Player victim )
		{
			ToastList.Instance.AddKillFeed( attacker, victim );
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

			RootPanel.AddChild<LongshotScope>();
			RootPanel.AddChild<RoundInfo>();

			var leftPanel = RootPanel.Add.Panel( "hud_left" );
			

			var centerPanel = RootPanel.Add.Panel("hud_center");
			centerPanel.AddChild<Vitals>();
			centerPanel.AddChild<Speedometer>();

			var rightPanel = RootPanel.Add.Panel("hud_right");
			rightPanel.AddChild<Ammo>();
			rightPanel.AddChild<WeaponList>();

			RootPanel.AddChild<Tokens>();
			RootPanel.AddChild<OutpostList>();
			
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<DamageIndicator>();
			RootPanel.AddChild<HitIndicator>();
			
			RootPanel.AddChild<Scoreboard>();
			RootPanel.AddChild<StationScreen>();
			RootPanel.AddChild<RespawnScreen>();
			RootPanel.AddChild<AwardQueue>();
			RootPanel.AddChild<ToastList>();
			RootPanel.AddChild<TutorialScreen>();
			RootPanel.AddChild<VictoryScreen>();
			RootPanel.AddChild<ChatBox>();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( Local.Pawn is Player player )
			{
				// TODO: Use a nice shader for this effect instead of this shit method.
				var healthScale = (0.4f / player.MaxHealth) * player.Health;

				RootPanel.Style.BackdropFilterSaturate = 0.6f + healthScale;
				RootPanel.Style.BackdropFilterContrast = 1.4f - healthScale;
				RootPanel.Style.Dirty();
			}
		}
	}
}
