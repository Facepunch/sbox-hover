
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public class KillFeedItem : Panel
	{
		public Label Attacker { get; set; }
		public Label Victim { get; set; }
		public Image Icon { get; set; }

		private float EndTime { get; set; }

		public KillFeedItem()
		{
			Attacker = Add.Label( "", "attacker" );
			Icon = Add.Image( "", "icon" );
			Victim = Add.Label( "", "victim" );
		}

		public void Update( Player victim )
		{
			Attacker.SetClass( "hidden", true );

			Victim.Text = victim.Client.Name;
			Victim.Style.FontColor = victim.Team.GetColor();
			Victim.Style.Dirty();

			Icon.Texture = Texture.Load( "ui/icons/skull.png" );

			EndTime = Time.Now + 3f;
		}

		public void Update( Entity attacker, Player victim )
		{
			if ( attacker is IKillFeedIcon killFeedIcon )
			{
				Attacker.Text = killFeedIcon.GetKillFeedName();
				Attacker.Style.FontColor = killFeedIcon.GetKillFeedTeam().GetColor();
				Attacker.Style.Dirty();

				Icon.Texture = Texture.Load( killFeedIcon.GetKillFeedIcon() );
			}
			else
			{
				Icon.Texture = Texture.Load( "ui/icons/skull.png" );
				Attacker.SetClass( "hidden", true );
			}

			Victim.Text = victim.Client.Name;
			Victim.Style.FontColor = victim.Team.GetColor();
			Victim.Style.Dirty();

			EndTime = Time.Now + 3f;
		}

		public void Update( Player attacker, Player victim, Entity weapon )
		{
			Attacker.Text = attacker.Client.Name;
			Attacker.Style.FontColor = attacker.Team.GetColor();
			Attacker.Style.Dirty();

			Victim.Text = victim.Client.Name;
			Victim.Style.FontColor = victim.Team.GetColor();
			Victim.Style.Dirty();

			if ( weapon.IsValid() )
			{
				if ( weapon is IKillFeedIcon killFeedIcon )
					Icon.Texture = Texture.Load( killFeedIcon.GetKillFeedIcon() );
				else if ( weapon is Weapon castWeapon )
					Icon.Texture = Texture.Load( castWeapon.Config.Icon );
				else
					Icon.Texture = Texture.Load( "ui/icons/skull.png" );
			}
			else
			{
				Icon.Texture = Texture.Load( "ui/icons/skull.png" );
			}

			EndTime = Time.Now + 3f;
		}

		public override void Tick()
		{
			if ( !IsDeleting && Time.Now >= EndTime )
				Delete();
		}
	}

	public class ToastItem : Panel
	{
		public Label Text { get; set; }
		public Image Icon { get; set; }

		private float EndTime { get; set; }

		public ToastItem()
		{
			Icon = Add.Image( "", "icon" );
			Text = Add.Label( "", "text" );
		}

		public void Update( string text, Texture icon = null )
		{
			Icon.Texture = icon;
			Text.Text = text;

			Icon.SetClass( "hidden", icon == null );

			EndTime = Time.Now + Game.ToastDuration;
		}

		public override void Tick()
		{
			if ( !IsDeleting && Time.Now >= EndTime )
				Delete();
		}
	}

	public class ToastList : Panel
	{
		public static ToastList Instance { get; private set; }

		public ToastList()
		{
			StyleSheet.Load( "/ui/ToastList.scss" );
			Instance = this;
		}

		public void AddKillFeed( Player attacker, Player victim, Entity weapon )
		{
			var item = AddChild<KillFeedItem>();
			item.Update( attacker, victim, weapon );
		}

		public void AddKillFeed( Entity attacker, Player victim )
		{
			var item = AddChild<KillFeedItem>();
			item.Update( attacker, victim );
		}

		public void AddKillFeed( Player victim )
		{
			var item = AddChild<KillFeedItem>();
			item.Update( victim );
		}

		public void AddItem( string text, Texture icon = null )
		{
			var item = AddChild<ToastItem>();
			item.Update( text, icon );
		}
	}
}
