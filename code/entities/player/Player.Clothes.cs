﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.Hover
{
	public partial class Player
	{
		protected List<ModelEntity> Clothing { get; set; } = new();

		public ModelEntity AttachClothing( string modelName )
		{
			var entity = new ModelEntity();

			entity.SetModel( modelName );
			entity.SetParent( this, true );
			entity.EnableShadowInFirstPerson = true;
			entity.EnableHideInFirstPerson = true;

			Clothing.Add( entity );

			return entity;
		}

		public void RemoveClothing()
		{
			Clothing.ForEach( ( entity ) => entity.Delete() );
			Clothing.Clear();
		}
	}
}
