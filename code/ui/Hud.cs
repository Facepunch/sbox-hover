
using Sandbox;
using Sandbox.Effects;
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
			ToastList.Instance.AddItem( text, Texture.Load( FileSystem.Mounted, icon ) );
		}

		private ScreenEffects PostProcessing { get; set; }

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

			PostProcessing = new();

			Map.Camera.AddHook( PostProcessing );
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( Local.Pawn is not Player player ) return;

			var pp = PostProcessing;

			pp.ChromaticAberration.Scale = 0.1f;
			pp.ChromaticAberration.Offset = Vector3.Zero;

			pp.Sharpen = 0.1f;

			var healthScale = (0.4f / player.MaxHealth) * player.Health;
			pp.Saturation = 0.7f + healthScale;

			pp.Vignette.Intensity = 0.8f - healthScale * 2f;
			pp.Vignette.Color = Color.Red.WithAlpha( 0.1f );
			pp.Vignette.Smoothness = 1f;
			pp.Vignette.Roundness = 0.8f;
		}
	}
}
