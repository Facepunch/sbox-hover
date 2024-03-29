﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
    public partial class FirstBloodAward : Award
	{
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/awards/first_blood.png" );
		public override string Name => "First Blood";
		public override string Description => "Be the first to kill an enemy player in a match";
		public override int Tokens => 500;
	}
}
