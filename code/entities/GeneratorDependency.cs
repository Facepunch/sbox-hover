﻿using Sandbox;

namespace Facepunch.Hover
{
	public partial class GeneratorDependency : AnimEntity, IHudEntity, IGameResettable
	{
		[Net, Change] public bool IsPowered { get; set; } = true;

		public EntityHudIcon NoPowerIcon { get; private set; }
		public EntityHudAnchor Hud { get; private set; }

		[Net, Property] public Team Team { get; set; }

		public Vector3 LocalCenter => CollisionBounds.Center;

		public virtual void OnGameReset()
		{
			IsPowered = true;
		}

		public virtual bool ShouldUpdateHud()
		{
			return true;
		}

		public virtual void UpdateHudComponents()
		{
			var distance = Local.Pawn.Position.Distance( Position ) - 1000f;
			var mapped = 1f - distance.Remap( 0f, 1000f, 0f, 1f );

			if ( Hud.Style.Opacity != mapped )
			{
				Hud.Style.Opacity = mapped;
				Hud.Style.Dirty();
			}
		}

		public override void Spawn()
		{
			GeneratorEntity.OnGeneratorBroken += OnGeneratorBroken;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Hud = EntityHud.Instance.Create( this );
			Hud.SetActive( false );

			NoPowerIcon = Hud.AddChild<EntityHudIcon>( "power" );
			NoPowerIcon.SetTexture( "ui/icons/no-power.png" );

			base.ClientSpawn();
		}

		protected override void OnDestroy()
		{
			GeneratorEntity.OnGeneratorBroken -= OnGeneratorBroken;

			base.OnDestroy();
		}

		protected virtual void OnGeneratorBroken( GeneratorEntity generator )
		{
			if ( generator.Team == Team )
			{
				IsPowered = false;
			}
		}

		protected virtual void OnIsPoweredChanged( bool isPowered )
		{
			Hud.SetActive( !isPowered );
		}
	}
}
