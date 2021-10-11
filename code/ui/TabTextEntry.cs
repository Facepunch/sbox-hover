
using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hover
{
	public partial class TabTextEntry : TextEntry
	{
		public event Action OnTabPressed;

		public override void OnButtonTyped( string button, KeyModifiers km )
		{
			if ( button == "tab" )
			{
				OnTabPressed?.Invoke();
				return;
			}

			base.OnButtonTyped( button, km );
		}
	}
}
