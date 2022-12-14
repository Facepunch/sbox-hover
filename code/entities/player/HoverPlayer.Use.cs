using Sandbox;
using System;

namespace Facepunch.Hover
{
	public partial class HoverPlayer
	{
		public Entity Using { get; protected set; }

		protected virtual void TickPlayerUse()
		{
			if ( !Game.IsServer ) return;

			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.Use ) )
				{
					Using = FindUsable();

					if ( Using == null )
					{
						UseFail();
						return;
					}
				}

				if ( !Input.Down( InputButton.Use ) )
				{
					StopUsing();
					return;
				}

				if ( !Using.IsValid() )
					return;

				if ( Using is IUse use && use.OnUse( this ) )
					return;

				StopUsing();
			}
		}

		protected virtual void UseFail()
		{
			//PlaySound( "player_use_fail" );
		}

		protected virtual void StopUsing()
		{
			Using = null;
		}

		protected bool IsValidUseEntity( Entity entity )
		{
			if ( entity == null ) return false;
			if ( entity is not IUse use ) return false;
			if ( !use.IsUsable( this ) ) return false;

			return true;
		}

		protected virtual Entity FindUsable()
		{
			var trace = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 150f )
				.Ignore( this )
				.Run();

			if ( !IsValidUseEntity( trace.Entity ) )
			{
				trace = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 150f )
				.Radius( 2 )
				.Ignore( this )
				.Run();
			}

			if ( !IsValidUseEntity( trace.Entity ) )
				return null;

			return trace.Entity;
		}
	}
}
