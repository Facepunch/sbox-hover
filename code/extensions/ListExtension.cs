using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.Hover;

public static class ListExtension
{
	public static List<T> Shuffle<T>( this List<T> self )
	{
		var count = self.Count;
		var last = count - 1;

		for ( var i = 0; i < last; ++i )
		{
			var r = Game.Random.Int( i, last );
			var tmp = self[i];
			self[i] = self[r];
			self[r] = tmp;
		}

		return self;
	}
}
