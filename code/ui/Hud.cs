using Sandbox;
using Sandbox.Effects;
using Sandbox.UI;

namespace Facepunch.Hover.UI;

public partial class Hud : RootPanel
{
	[ClientRpc]
	public static void AddKillFeed( HoverPlayer attacker, HoverPlayer victim, Entity weapon )
	{
		ToastList.Instance.AddKillFeed( attacker, victim, weapon );
	}

	[ClientRpc]
	public static void AddKillFeed( Entity attacker, HoverPlayer victim )
	{
		ToastList.Instance.AddKillFeed( attacker, victim );
	}

	[ClientRpc]
	public static void AddKillFeed( HoverPlayer victim )
	{
		ToastList.Instance.AddKillFeed( victim );
	}

	public static void ToastAll( string text, string icon = "" )
	{
		Toast( To.Everyone, text, icon );
	}

	public static void Toast( HoverPlayer player, string text, string icon = "" )
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
		StyleSheet.Load( "/ui/Hud.scss" );

		AddChild<LongshotScope>();
		AddChild<RoundInfo>();

		var leftPanel = Add.Panel( "hud_left" );

		var centerPanel = Add.Panel("hud_center");
		centerPanel.AddChild<Vitals>();
		centerPanel.AddChild<Speedometer>();

		var rightPanel = Add.Panel("hud_right");
		rightPanel.AddChild<Ammo>();
		rightPanel.AddChild<WeaponList>();

		AddChild<Tokens>();
		AddChild<OutpostList>();
		
		AddChild<VoiceList>();
		AddChild<DamageIndicator>();
		AddChild<HitIndicator>();
		
		AddChild<Scoreboard>();
		AddChild<StationScreen>();
		AddChild<RespawnScreen>();
		AddChild<AwardQueue>();
		AddChild<ToastList>();
		AddChild<TutorialScreen>();
		AddChild<VictoryScreen>();
		AddChild<ChatBox>();

		PostProcessing = new();

		Camera.Main.AddHook( PostProcessing );
	}

	[Event.Tick.Client]
	private void ClientTick()
	{
		if ( Local.Pawn is not HoverPlayer player ) return;

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
