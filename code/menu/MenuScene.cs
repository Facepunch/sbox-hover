using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.Hover;

public class MenuScene : ScenePanel
{
	private SceneSpotLight Light { get; set; }
	private List<SceneModel> Models { get; set; } = new();
	private SceneParticles Particles { get; set; }
	private SceneModel Jetpack { get; set; }
	private SceneModel Citizen { get; set; }
	
	public override void Delete( bool immediate = false )
	{
		World?.Delete();
		World = null;
		
		base.Delete( immediate );
	}

	public override void Tick()
	{
		var direction = (Vector3.Backward + Vector3.Right * 0.5f).Normal;

		Citizen.Rotation = Rotation.FromYaw( -25f );
		Citizen.Position = new( 0f, 0f, MathF.Sin( RealTime.Now * 2f ) * 5f );
		
		Camera.Position = Vector3.Forward * 200f + Vector3.Left * 30f + Vector3.Up * 35f;
		Camera.Rotation = Rotation.LookAt( direction, Vector3.Up );

		var dt = Game.InGame ? Time.Delta : RealTime.Delta;
		
		foreach ( var model in Models )
		{
			model.Update( dt );
		}

		World.AmbientLightColor = Color.White.Darken( 0.5f );

		var attachment = Jetpack.GetAttachment( "trail" );
		
		if ( attachment is not null )
		{
			Particles.SetControlPoint( 0, attachment.Value.Position );
		}

		Particles.Simulate( dt );
		
		base.Tick();
	}

	protected override void OnParametersSet()
	{
		World = new();
		Light = new( World );
		Citizen = new( World, "models/citizen/citizen.vmdl", new( Vector3.Zero, Rotation.Identity ) );
		Models.Add( Citizen );
		
		var allClothing = ResourceLibrary.GetAll<Clothing>();
		var clothingToWear = new List<string>()
		{
			"light_shoes",
			"light_helmet",
			"light_chest",
			"light_gloves",
			"light_legs"
		};

		Game.SetRandomSeed( RealTime.Now.CeilToInt() );
		
		var team = Game.Random.Int( 0, 1 ) == 0 ? Team.Red : Team.Blue;

		foreach ( var assetName in clothingToWear )
		{
			var modelName = allClothing
				.Where( c => string.Equals( c.ResourceName, assetName, StringComparison.CurrentCultureIgnoreCase ) )
				.Select( c => c.Model )
				.FirstOrDefault();

			var clothing = new SceneModel( World, modelName, Transform.Zero )
			{
				ColorTint = team.GetColor()
			};
			
			clothing.SetMaterialGroup( team == Team.Red ? "red" : "blue" );

			Citizen.AddChild( "clothes", clothing );

			Models.Add( clothing );
		}

		Jetpack = new( World, "models/tempmodels/jetpack/jetpack.vmdl", Transform.Zero );
		Citizen.AddChild( "clothing", Jetpack );

		Particles = new( World, "particles/jetpack/jetpack_trail.vpcf" )
		{
			RenderParticles = true
		};

		Models.Add( Jetpack );
		
		Camera.FieldOfView = 60f;
	}
}

