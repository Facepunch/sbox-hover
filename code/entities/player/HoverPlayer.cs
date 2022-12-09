using Gamelib.Extensions;
using Sandbox;
using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class HoverPlayer : AnimatedEntity
	{
		public Dictionary<string,List<WeaponUpgrade>> WeaponUpgrades { get; private set; }
		public ProjectileSimulator Projectiles { get; private set; }
		public HashSet<Type> LoadoutUpgrades { get; private set; }
		public List<Award> EarnedAwards { get; private set; }
		public TimeSince LastKillTime { get; private set; }
		public int SuccessiveKills { get; private set; }

		private FirstPersonCamera FirstPersonCamera { get; set; }
		private SpectateCamera SpectateCamera { get; set; }

		public SceneModel AnimatedLegs { get; private set; }

		private HashSet<string> LegBonesToKeep = new()
		{
			"leg_upper_R_twist",
			"leg_upper_R",
			"leg_upper_L",
			"leg_upper_L_twist",
			"leg_lower_L",
			"leg_lower_R",
			"ankle_L",
			"ankle_R",
			"ball_L",
			"ball_R",
			"leg_knee_helper_L",
			"leg_knee_helper_R",
			"leg_lower_R_twist",
			"leg_lower_L_twist"
		};

		public Vector3 EyePosition
		{
			get => Transform.PointToWorld( EyeLocalPosition );
			set => EyeLocalPosition = Transform.PointToLocal( value );
		}

		[Net, Predicted]
		public Vector3 EyeLocalPosition { get; set; }

		public Rotation EyeRotation
		{
			get => Transform.RotationToWorld( EyeLocalRotation );
			set => EyeLocalRotation = Transform.RotationToLocal( value );
		}

		[Net, Predicted]
		public Rotation EyeLocalRotation { get; set; }

		public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

		private class LegsClothingObject
		{
			public SceneModel SceneObject { get; set; }
			public Clothing Asset { get; set; }
		}

		private Entity LastActiveChild { get; set; }
		private List<LegsClothingObject> LegsClothing { get; set; } = new();

		[ConCmd.Server]
		public static void BuyWeaponUpgrade( string configName, string upgradeName )
		{
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
			{
				var config = TypeLibrary.Create<WeaponConfig>( configName );
				var upgradeDesc = TypeLibrary.GetType<WeaponUpgrade>( upgradeName );

				if ( upgradeDesc == null ) return;

				if ( config.Upgrades != null && config.Upgrades.Contains( upgradeDesc.TargetType ) )
				{
					var upgrade = TypeLibrary.Create<WeaponUpgrade>( upgradeDesc.TargetType );

					if ( player.HasTokens( upgrade.TokenCost) )
					{
						player.GiveWeaponUpgrade( config, upgrade );
						player.TakeTokens( upgrade.TokenCost );

						var weapons = player.Children
							.OfType<Weapon>()
							.Where( v => v.Config.ClassName == config.ClassName );

						foreach ( var weapon in weapons )
						{
							upgrade.Apply( player, weapon );
						}

						player.Restock();
					}
				}
			}
		}

		[ConCmd.Server]
		public static void BuyLoadoutUpgrade( string loadoutName, string weapons )
		{
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
			{
				var loadoutDesc = TypeLibrary.GetType<BaseLoadout>( loadoutName );

				if ( loadoutDesc != null )
				{
					var loadout = TypeLibrary.Create<BaseLoadout>( loadoutDesc.TargetType );
					if ( loadout == null ) return;

					if ( player.HasTokens( loadout.UpgradeCost ) )
					{
						player.GiveLoadoutUpgrade( loadoutDesc.TargetType );
						player.TakeTokens( loadout.UpgradeCost );

						if ( player.Loadout.UpgradesTo == loadoutDesc.TargetType )
						{
							// This is a direct upgrade to what we have already.
							player.GiveLoadout( loadout );

							loadout.UpdateWeapons( weapons.Split( ',' ) );
							loadout.Respawn( player );
							loadout.Supply( player );
						}

						UI.StationScreen.Refresh( To.Single( player ) );
					}
				}
			}
		}

		[ConCmd.Server]
		public static void SwitchTeam()
		{
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
			{
				if ( player.LastTeamSwitchTime > 10f )
				{
					var targetTeam = Team.Red;
					var currentTeam = player.Team;

					if ( currentTeam == Team.Red )
						targetTeam = Team.Blue;

					var targetPlayers = targetTeam.GetCount();
					var currentPlayers = currentTeam.GetCount();

					if ( targetPlayers == currentPlayers )
					{
						UI.Hud.Toast( player, "The teams are already balanced!" );
						return;
					}

					if ( targetPlayers > currentPlayers )
					{
						UI.Hud.Toast( player, "There are too many players on that team!" );
						return;
					}

					player.LastTeamSwitchTime = 0f;
					player.SetTeam( targetTeam );
					player.Respawn();
				}
				else
				{
					UI.Hud.Toast( player, "You cannot change teams yet!" );
				}
			}
		}

		[ConCmd.Server]
		public static void ChangeLoadout( string loadoutName, string weapons )
		{
			if ( ConsoleSystem.Caller.Pawn is HoverPlayer player )
			{
				var loadoutDesc = TypeLibrary.GetType<BaseLoadout>( loadoutName );

				if ( loadoutDesc != null )
				{
					var loadout = TypeLibrary.Create<BaseLoadout>( loadoutDesc.TargetType );
					if ( loadout == null ) return;

					loadout.UpdateWeapons( weapons.Split( ',' ) );
					player.GiveLoadout( loadout );

					if ( player.LifeState == LifeState.Alive )
					{
						loadout.Respawn( player );
						loadout.Supply( player );

						UI.WeaponList.Expand( To.Single( player ), 4f );
					}
					else
					{
						player.Respawn();
					}
				}
			}
		}

		private class AssistTracker
		{
			public TimeSince LastDamageTime { get; set; }
			public HoverPlayer Attacker { get; set; }
			public float TotalDamage { get; set; }
		}

		[Net, Predicted] public MoveController Controller { get; set; }
		[Net, Predicted] public Entity ActiveChild { get; set; }
		[ClientInput] public Vector3 InputDirection { get; protected set; }
		[ClientInput] public Entity ActiveChildInput { get; set; }
		[ClientInput] public Angles ViewAngles { get; set; }
		public Angles OriginalViewAngles { get; private set; }

		[Net] public RealTimeUntil NextStationRestock { get; set; }
		[Net] public RealTimeUntil ShouldHideOnRadar { get; set; }
		[Net] public TimeSince TimeSinceSpawn { get; private set; }
		[Net] public bool InDeployableBlocker { get; set; }
		[Net] public float TargetAlpha { get; set; } = 1f;
		[Net, Local] public int Tokens { get; set; }
		[Net] public float HealthRegen { get; set; }
		[Net] public float RegenDelay { get; set; }
		[Net] public RealTimeUntil NextRegenTime { get; set; }
		[Net] public int KillStreak { get; set; }
		[Net] public float MaxHealth { get; set; }

		[Net] public bool InEnergyElevator { get; set; }
		[Net] public bool IsRegeneratingEnergy { get; set; }
		[Net] public float MaxEnergy { get; set; }
		[Net] public float Energy { get; set; }
		[Net] public float EnergyRegen { get; set; } = 20f;
		[Net] public float EnergyDrain { get; set; } = 20f;

		public TimeSince LastTeamSwitchTime { get; private set; }
		public RealTimeUntil? RespawnTime { get; private set; }
		public DamageInfo LastDamageInfo { get; private set; }
		public Inventory Inventory { get; private set; }
		public HoverPlayer LastKiller { get; set; }

		private List<AssistTracker> AssistTrackers { get; set; }
		private Rotation LastCameraRotation { get; set; }
		private Particles SpeedLines { get; set; }
		private UI.Nameplate Nameplate { get; set; }
		private UI.Radar RadarHud { get; set; }
		private bool PlayLowEnergySound { get; set; }
		private bool IsPlayingJetpackLoop { get; set; }
		private bool IsPlayingWindLoop { get; set; }
		private bool IsPlayingSkiLoop { get; set; }
		private bool IsRegenerating { get; set; }
		private Sound JetpackLoop { get; set; }
		private Sound WindLoop { get; set; }
		private Sound SkiLoop { get; set; }
		private float WalkBob { get; set; }
		private float FOV { get; set; }

		public bool HasTeam
		{
			get => Team != Team.None;
		}

		public HoverPlayer()
		{
			FirstPersonCamera = new();
			SpectateCamera = new();
			LoadoutUpgrades = new();
			WeaponUpgrades = new();
			AssistTrackers = new();
			EarnedAwards = new();
			Projectiles = new( this );
			EnableTouch = true;
			Inventory = new Inventory( this );
		}

		public bool IsSpectator
		{
			get => LifeState == LifeState.Dead;
		}

		public void Reset()
		{
			Client.SetInt( "captures", 0 );
			Client.SetInt( "kills", 0 );
			Client.SetInt( "kills", 0 );

			LoadoutUpgrades.Clear();
			WeaponUpgrades.Clear();
			Projectiles.Clear();
			EarnedAwards.Clear();
			LastDamageInfo = default;
			LastKiller = null;
			Tokens = Game.StartingTokens;
			Team = Team.None;

			ResetClient( To.Single( this ) );
		}

		public T GetWeapon<T>() where T : Weapon
		{
			return Children.OfType<T>().FirstOrDefault();
		}

		public bool HasWeapon<T>() where T : Weapon
		{
			return Children.OfType<T>().Any();
		}

		[ClientRpc]
		public void ResetClient()
		{
			LoadoutUpgrades.Clear();
			WeaponUpgrades.Clear();
			EarnedAwards.Clear();
			Projectiles.Clear();
		}

		public void GiveTokens( int tokens )
		{
			Tokens += tokens;
		}

		public void TakeTokens( int tokens )
		{
			Tokens = Math.Max( Tokens - tokens, 0 );
		}

		public bool HasTokens( int tokens )
		{
			return (Tokens >= tokens);
		}

		public bool HasWeaponUpgrade( Weapon weapon, Type type )
		{
			var weaponName = weapon.Config.ClassName;

			if ( WeaponUpgrades.TryGetValue( weaponName, out var set ) )
			{
				foreach ( var upgrade in set )
				{
					if ( upgrade.GetType() == type )
					{
						return true;
					}
				}
			}

			return false;
		}

		public void ApplyWeaponUpgrades()
		{
			foreach ( var weapon in Children.OfType<Weapon>() )
			{
				var upgrades = GetWeaponUpgrades( weapon );

				if ( upgrades != null )
				{
					foreach ( var upgrade in upgrades )
					{
						upgrade.Apply( this, weapon );
					}
				}
			}
		}

		public void RestockWeaponUpgrades()
		{
			foreach ( var weapon in Children.OfType<Weapon>() )
			{
				var upgrades = GetWeaponUpgrades( weapon );

				if ( upgrades != null )
				{
					foreach ( var upgrade in upgrades )
					{
						upgrade.Restock( this, weapon );
					}
				}
			}
		}

		public List<WeaponUpgrade> GetWeaponUpgrades( WeaponConfig config )
		{
			if ( WeaponUpgrades.TryGetValue( config.ClassName, out var upgrades ) )
			{
				return upgrades;
			}

			return null;
		}

		public List<WeaponUpgrade> GetWeaponUpgrades( Weapon weapon )
		{
			return GetWeaponUpgrades( weapon.Config );
		}

		public void GiveWeaponUpgrade( string weaponName, WeaponUpgrade upgrade )
		{
			if ( WeaponUpgrades.TryGetValue( weaponName, out var upgrades ) )
			{
				GiveWeaponUpgrade( To.Single( this ), weaponName, upgrade.GetType().Name );
				upgrades.Add( upgrade );
				return;
			}

			upgrades = new List<WeaponUpgrade>();
			WeaponUpgrades[weaponName] = upgrades;
			upgrades.Add( upgrade );

			GiveWeaponUpgrade( To.Single( this ), weaponName, upgrade.GetType().Name );
		}

		public void GiveWeaponUpgrade( WeaponConfig config, WeaponUpgrade upgrade )
		{
			GiveWeaponUpgrade( config.ClassName, upgrade );
		}

		public void GiveWeaponUpgrade( Weapon weapon, WeaponUpgrade upgrade )
		{
			GiveWeaponUpgrade( weapon.Config, upgrade );
		}

		[ClientRpc]
		public void GiveWeaponUpgrade( string weaponName, string typeName )
		{
			var upgrade = TypeLibrary.Create<WeaponUpgrade>( typeName );

			if ( WeaponUpgrades.TryGetValue( weaponName, out var upgrades ) )
			{
				upgrades.Add( upgrade );
				UI.StationScreen.Refresh();
				return;
			}

			upgrades = new List<WeaponUpgrade>();
			WeaponUpgrades[weaponName] = upgrades;
			upgrades.Add( upgrade );
			UI.StationScreen.Refresh();
		}

		public bool HasLoadoutUpgrade( Type type )
		{
			return LoadoutUpgrades.Contains( type );
		}

		public bool HasLoadoutUpgrade<T>() where T : BaseLoadout
		{
			return LoadoutUpgrades.Contains( typeof( T ) );
		}

		public void GiveLoadoutUpgrade( Type type )
		{
			LoadoutUpgrades.Add( type );
			GiveLoadoutUpgrade( To.Single( this ), type.Name );
		}

		[ClientRpc]
		public void GiveLoadoutUpgrade( string typeName )
		{
			var desc = TypeLibrary.GetType<BaseLoadout>( typeName );

			if ( desc != null )
			{
				LoadoutUpgrades.Add( desc.TargetType );
				UI.StationScreen.Refresh();
			}
		}

		public void GiveAward( string type )
		{
			var award = Awards.Get( type );
			if ( award == null ) return;

			if ( award.TeamReward )
			{
				foreach ( var member in Team.GetAll() )
				{
					member.GiveTokens( award.Tokens );
					member.ShowAward( To.Single( member ), type, this );
				}
			}
			else
			{
				GiveTokens( award.Tokens );
				ShowAward( To.Single( this ), type, this );
			}

			EarnedAwards.Add( award );
		}


		public void GiveAward<T>() where T : Award
		{
			GiveAward( typeof( T ).Name );
		}

		public void ApplyForce( Vector3 force )
		{
			if ( Controller is MoveController controller )
			{
				controller.Impulse += force;
			}
		}

		[ClientRpc]
		public void ShowAward( string name, HoverPlayer awardee )
		{
			var award = Awards.Get( name );
			if ( award == null ) return;

			if ( awardee == this )
			{
				EarnedAwards.Add( award );
			}

			award.Show();
		}

		public bool TryRestock()
		{
			if ( !NextStationRestock )
				return false;

			Restock();

			return true;
		}

		public void Restock()
		{
			var loadout = Loadout;

			if ( loadout != null )
			{
				PlaySound( $"weapon.pickup{Rand.Int( 1, 4 )}" );
				loadout.Restock( this );
			}

			NextStationRestock = 30f;
			NextRegenTime = 0f;
		}

		public HoverPlayer GetBestAssist( Entity attacker )
		{
			var minDamageTarget = MaxHealth * 0.3f;
			var assister = (HoverPlayer)null;
			var damage = 0f;

			foreach ( var tracker in AssistTrackers )
			{
				if ( tracker.Attacker != attacker && tracker.TotalDamage >= minDamageTarget )
				{
					if ( assister == null || tracker.TotalDamage > damage )
					{
						assister = tracker.Attacker;
						damage = tracker.TotalDamage;
					}
				}
			}

			return assister;
		}

		public void MakeSpectator( Vector3 position, float? respawnTime = null )
		{
			if ( respawnTime.HasValue )
				RespawnTime = respawnTime;
			else
				RespawnTime = null;

			EnableAllCollisions = false;
			EnableDrawing = false;
			LifeState = LifeState.Dead;
			Controller = null;
		}

		public virtual bool CanSelectWeapon( Weapon weapon )
		{
			return !weapon.IsPassive && weapon.IsAvailable();
		}

		[ClientRpc]
		public virtual void ClientRespawn()
		{
			StopJetpackLoop();
			StopSkiLoop();
		}

		public virtual void Respawn()
		{
			RemoveRagdollEntity();
			ClearAssistTrackers();

			SuccessiveKills = 0;
			TimeSinceSpawn = 0f;
			KillStreak = 0;
			LifeState = LifeState.Alive;
			Health = 100f;
			Velocity = Vector3.Zero;

			Game.Round?.OnPlayerSpawn( this );

			UI.WeaponList.Expand( To.Single( this ), 4f );

			ClientRespawn();
			CreateHull();

			GameManager.Current?.MoveToSpawnpoint( this );
			ResetInterpolation();
		}

		public override void OnNewModel( Model model )
		{
			if ( IsLocalPawn )
			{
				if ( AnimatedLegs is not null )
				{
					AnimatedLegs.RenderingEnabled = false;
					AnimatedLegs.Delete();
					AnimatedLegs = null;
				}

				AnimatedLegs = new( Map.Scene, model, Transform );
				AnimatedLegs.SetBodyGroup( "Head", 1 );

				foreach ( var clothing in LegsClothing )
				{
					clothing.SceneObject.Delete();
				}

				LegsClothing.Clear();

				foreach ( var child in Children )
				{
					AddClothingToLegs( child );
				}
			}

			base.OnNewModel( model );
		}

		public override void Spawn()
		{
			EnableLagCompensation = true;

			Tags.Add( "player" );

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			if ( IsLocalPawn )
			{
				SpeedLines = Particles.Create( "particles/player/speed_lines.vpcf" );
				RadarHud = Local.Hud.AddChild<UI.Radar>();
			}
			else
			{
				Nameplate = new UI.Nameplate( this );
			}

			base.ClientSpawn();
		}

		public override void StartTouch( Entity other )
		{
			if ( other is JetpackElevator )
			{
				InEnergyElevator = true;
			}

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( other is JetpackElevator )
			{
				InEnergyElevator = false;
			}

			base.EndTouch( other );
		}

		public override void OnChildAdded( Entity child )
		{
			Inventory?.OnChildAdded( child );
			AddClothingToLegs( child );
		}

		public override void OnChildRemoved( Entity child )
		{
			Inventory?.OnChildRemoved( child );

			if ( AnimatedLegs.IsValid() && child is ModelEntity model && child is not Weapon )
			{
				if ( model.Model == null ) return;

				var indexOf = LegsClothing.FindIndex( 0, c => c.Asset.Model.ToLower() == model.Model.Name.ToLower() );

				if ( indexOf >= 0 )
				{
					var clothing = LegsClothing[indexOf];
					LegsClothing.RemoveAt( indexOf );
					clothing.SceneObject.Delete();
				}
			}
		}

		public override void OnKilled()
		{
			LifeState = LifeState.Dead;
			StopUsing();

			Client?.AddInt( "deaths", 1 );

			var attacker = LastAttacker;

			if ( attacker.IsValid() )
			{
				if ( attacker is HoverPlayer killer )
				{
					killer.OnKillPlayer( this, LastDamageInfo );
				}

				Game.Round?.OnPlayerKilled( this, attacker, LastDamageInfo );
			}
			else
			{
				Game.Round?.OnPlayerKilled( this, null, LastDamageInfo );
			}


			BecomeRagdollOnClient( LastDamageInfo.Force, LastDamageInfo.BoneIndex );
			Inventory.DeleteContents();

			if ( LastDamageInfo.HasTag( "fall" ) )
			{
				PlaySound( "player.falldie" );
			}

			var bloodExplosion = Particles.Create( "particles/blood/explosion_blood/explosion_blood.vpcf", Position );
			bloodExplosion.SetForward( 0, LastDamageInfo.Force.Normal );

			UI.StationScreen.Hide( To.Single( this ) );

			PlaySound( $"grunt{Rand.Int( 1, 4 )}" );

			SuccessiveKills = 0;
			KillStreak = 0;

			OnClientKilled();
		}

		public override void BuildInput()
		{
			var stationScreen = UI.StationScreen.Instance;

			if ( stationScreen.IsOpen )
			{
				if ( stationScreen.Mode == UI.StationScreenMode.Station && Input.Released( InputButton.Use ) )
				{
					UI.StationScreen.Hide();
				}

				Input.StopProcessing = true;
				Input.ClearButtons();
				Input.AnalogMove = Vector3.Zero;
				Input.AnalogLook = Angles.Zero;

				return;
			}

			OriginalViewAngles = ViewAngles;
			InputDirection = Input.AnalogMove;

			if ( Input.StopProcessing )
				return;

			var look = Input.AnalogLook;

			if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
			{
				look = look.WithYaw( look.yaw * -1f );
			}

			var viewAngles = ViewAngles;
			viewAngles += look;
			viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
			viewAngles.roll = 0f;
			ViewAngles = viewAngles.Normal;

			ActiveChild?.BuildInput();
		}

		public override void FrameSimulate( Client client )
		{
			if ( LifeState == LifeState.Alive )
			{
				Controller?.SetActivePlayer( this );
				Controller?.FrameSimulate();
			}

			if ( LifeState == LifeState.Alive )
				FirstPersonCamera?.Update();
			else
				SpectateCamera?.Update();

			base.FrameSimulate( client );
		}

		public override void Simulate( Client client )
		{
			Projectiles.Simulate();

			if ( LifeState == LifeState.Alive )
			{
				Controller?.SetActivePlayer( this );
				Controller?.Simulate();

				SimulateActiveChild( ActiveChild );
				SimulateAnimation();
			}

			var targetWeapon = ActiveChildInput as Weapon;

			if ( targetWeapon.IsValid() && ActiveChild != targetWeapon )
			{
				if ( CanSelectWeapon( targetWeapon ) )
				{
					PlaySound( $"weapon.pickup{Rand.Int( 1, 4 )}" );
					ActiveChild = targetWeapon;
				}
				else
				{
					var firstWeapon = Children.OfType<Weapon>().Where( CanSelectWeapon ).FirstOrDefault();

					if ( ActiveChild != firstWeapon )
					{
						PlaySound( $"weapon.pickup{Rand.Int( 1, 4 )}" );
						ActiveChild = firstWeapon;
					}
				}
			}

			if ( LifeState != LifeState.Alive )
				return;

			if ( IsServer && Input.Released( InputButton.Drop ) )
			{
				var spottedPlayers = 0;
				var trace = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 20000f )
					.Ignore( this )
					.Run();

				foreach ( var player in All.OfType<HoverPlayer>() )
				{
					if ( !IsEnemyPlayer( player ) )
						continue;

					if ( player.Position.DistanceToLine( trace.StartPosition, trace.EndPosition, out var _ ) > 200f )
						continue;

					var visibleTrace = Trace.Ray( EyePosition, EyePosition + (player.WorldSpaceBounds.Center - EyePosition).Normal * 20000f )
						.Ignore( this )
						.Run();

					if ( visibleTrace.Entity == player )
					{
						player.ShouldHideOnRadar = 4f;
						spottedPlayers++;
					}
				}

				if ( spottedPlayers > 0 )
				{
					Audio.Play( To.Single( this ), "hover.hoversharp" );
				}
			}

			foreach ( var child in Children )
			{
				if ( child is Equipment equipment && equipment.AbilityButton.HasValue )
				{
					if ( Input.Released( equipment.AbilityButton.Value )  )
					{
						equipment.OnAbilityUsed();
					}
				}
			}

			if ( IsServer && Input.Released( InputButton.Use ) )
			{
				var station = FindInSphere( Position, 50f )
					.OfType<StationAsset>()
					.FirstOrDefault();

				if ( station != null && station.CanPlayerUse( this ) )
				{
					using ( Prediction.Off() )
					{
						station.ShowUseEffects();
						UI.StationScreen.Show( To.Single( this ), UI.StationScreenMode.Station );
					}
				}
			}

			if ( IsServer && Input.Released( InputButton.Drop ) )
			{
				foreach ( var flag in All.OfType<FlagEntity>() )
				{
					if ( flag.Carrier == this )
					{
						flag.Drop( true );
						break;
					}
				}
			}

			TickPlayerUse();
		}

		private void AddClothingToLegs( Entity child )
		{
			if ( !AnimatedLegs.IsValid() || child is not ModelEntity model || child is Weapon )
				return;

			if ( model.Model == null ) return;

			var assets = ResourceLibrary.GetAll<Clothing>();
			var asset = assets.FirstOrDefault( a => !string.IsNullOrEmpty( a.Model ) && a.Model.ToLower() == model.Model.Name.ToLower() );
			if ( asset == null ) return;

			if ( asset.Category == Sandbox.Clothing.ClothingCategory.Bottoms
				|| asset.Category == Sandbox.Clothing.ClothingCategory.Footwear
				|| asset.Category == Sandbox.Clothing.ClothingCategory.Tops )
			{
				var clothing = new SceneModel( Map.Scene, model.Model, AnimatedLegs.Transform );
				AnimatedLegs.AddChild( "clothing", clothing );

				LegsClothing.Add( new()
				{
					SceneObject = clothing,
					Asset = asset
				} );
			}
		}

		[Event.Client.PostCamera]
		private void PostCameraSetup()
		{
			if ( LastCameraRotation == Rotation.Identity )
				LastCameraRotation = Camera.Rotation;

			var angleDiff = Rotation.Difference( LastCameraRotation, Camera.Rotation );
			var angleDiffDegrees = angleDiff.Angle();
			var allowance = 20.0f;

			if ( angleDiffDegrees > allowance )
			{
				LastCameraRotation = Rotation.Lerp( LastCameraRotation, Camera.Rotation, 1.0f - (allowance / angleDiffDegrees) );
			}
			
			AddCameraEffects();
		}

		private void ClearAssistTrackers()
		{
			AssistTrackers.Clear();
		}

		private AssistTracker GetAssistTracker( HoverPlayer attacker )
		{
			foreach ( var v in AssistTrackers )
			{
				if ( v.Attacker == attacker )
				{
					return v;
				}
			}

			var tracker = new AssistTracker
			{
				LastDamageTime = 0f,
				Attacker = attacker
			};

			AssistTrackers.Add( tracker );

			return tracker;
		}

		private void AddAssistDamage( HoverPlayer attacker, DamageInfo info )
		{
			var tracker = GetAssistTracker( attacker );

			if ( tracker.LastDamageTime > 10f )
			{
				tracker.TotalDamage = 0f;
			}

			tracker.LastDamageTime = 0f;
			tracker.TotalDamage += info.Damage;
		}

		private void AddCameraEffects()
		{
			if ( Controller is not MoveController controller )
				return;

			var forwardSpeed = Velocity.Normal.Dot( Camera.Rotation.Forward );
			var speed = Velocity.Length.LerpInverse( 0, controller.MaxSpeed );
			var left = Camera.Rotation.Left;
			var up = Camera.Rotation.Up;

			if ( GroundEntity != null )
			{
				WalkBob += Time.Delta * 25f * speed;
			}

			Camera.Position += up * MathF.Sin( WalkBob ) * speed * 2f;
			Camera.Position += left * MathF.Sin( WalkBob * 0.6f ) * speed * 1f;

			speed = (speed - 0.7f).Clamp( 0f, 1f ) * 3f;

			FOV = FOV.LerpTo( speed * 30f * MathF.Abs( forwardSpeed ), Time.Delta * 2f );

			Camera.FieldOfView += (FOV * 0.2f);
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( info.Hitbox.HasTag( "head" ) && !info.HasTag( "blast" ) )
			{
				info.Damage *= 2.0f;
			}

			if ( Controller is MoveController controller )
			{
				controller.Impulse += info.Force;
			}

			if ( info.Attacker is HoverPlayer attacker && attacker != this )
			{
				// We can't take damage from our own team.
				if ( attacker.Team == Team && !Game.AllowFriendlyFire )
				{
					return;
				}

				// We can't take damage from others during the spawn protection period.
				if ( TimeSinceSpawn < 3f )
				{
					return;
				}

				if ( info.Weapon is Weapon weapon )
				{
					var upgrades = attacker.GetWeaponUpgrades( weapon );

					if ( upgrades != null )
					{
						foreach ( var upgrade in upgrades )
						{
							info = upgrade.DealDamage( attacker, this, weapon, info );
						}
					}
				}

				if ( info.HasTag( "blunt" ) )
				{
					var dotDirection = EyeRotation.Forward.Dot( attacker.EyeRotation.Forward );

					if ( dotDirection >= 0.5f )
					{
						info.Damage *= 4f;
					}
				}

				if ( info.Damage > 0 )
				{
					AddAssistDamage( attacker, info );
					attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, ((float)Health).LerpInverse( 100f, 0f ) );
				}
			}

			foreach ( var equipment in Children.OfType<Equipment>() )
			{
				info = equipment.OwnerTakeDamage( info );
			}

			if ( info.Damage > 0 )
			{
				ShowFloatingDamage( info.Damage, info.Position );
			}

			var fromPosition = info.Position;

			if ( info.Weapon.IsValid() )
				fromPosition = info.Weapon.Position;
			else if ( info.Attacker.IsValid() )
				fromPosition = info.Attacker.Position;

			TookDamage( To.Single( this ), fromPosition, info.Damage );

			var bloodSplat = Particles.Create( "particles/blood/large_blood/large_blood.vpcf", info.Position );
			bloodSplat.SetForward( 0, info.Force.Normal );

			// Don't play grunt sounds too often - it can be annoying.
			if ( Rand.Float() >= 0.5f )
			{
				PlaySound( "grunt" + Rand.Int( 1, 4 ) );
			}

			IsRegenerating = false;
			LastDamageInfo = info;
			NextRegenTime = RegenDelay;

			if ( LifeState == LifeState.Alive )
			{
				base.TakeDamage( info );

				this.ProceduralHitReaction( info );

				if ( LifeState == LifeState.Dead && info.Attacker.IsValid() )
				{
					if ( info.Attacker.Client.IsValid() && info.Attacker.IsValid() )
					{
						info.Attacker.Client.AddInt( "kills" );
					}
				}
			}
		}

		[ClientRpc]
		public void ShowFloatingDamage( float damage, Vector3 position )
		{
			// Don't show damage that happened to us.
			if ( IsLocalPawn ) return;

			var panel = UI.FloatingDamage.Rent();

			panel.SetLifeTime( Rand.Float( 2f, 3f ) );
			panel.SetDamage( damage );
			panel.Velocity = Vector3.Up * Rand.Float( 30f, 50f ) + Vector3.Random * Rand.Float( 50f, 125f );
			panel.Position = position;
		}

		[ClientRpc]
		public void RemoveRagdollOnClient()
		{
			RemoveRagdollEntity();
		}

		public void RemoveRagdollEntity()
		{
			if ( IsServer )
			{
				RemoveRagdollOnClient();
			}

			if ( Ragdoll != null && Ragdoll.IsValid() )
			{
				Ragdoll.Delete();
				Ragdoll = null;
			}
		}

		[ClientRpc]
		public void DidDamage( Vector3 position, float amount, float inverseHealth )
		{
			Sound.FromScreen( "hitmarker" ).SetPitch( 1f + inverseHealth * 1f );
			UI.HitIndicator.Current?.OnHit( position, amount );
		}

		[ClientRpc]
		public void TookDamage( Vector3 position, float amount )
		{
			UI.DamageIndicator.Current?.OnHit( position );
		}

		public virtual bool IsEnemyPlayer( HoverPlayer other )
		{
			return other.Team != Team;
		}

		public virtual void OnDeployablePickedUp( DeployableEntity entity )
		{
			foreach ( var equipment in Children.OfType<Equipment>() )
			{
				equipment.OnDeployablePickedUp( entity );
			}
		}

		public virtual void OnCaptureFlag( FlagEntity flag )
		{
			Client.SetInt( "captures", Client.GetInt( "captures", 0 ) + 1 );
			GiveAward<CaptureFlagAward>();
		}

		public virtual void OnReturnFlag( FlagEntity flag )
		{
			GiveAward<ReturnFlagAward>();
		}

		public virtual void OnKillPlayer( HoverPlayer victim, DamageInfo damageInfo )
		{
			if ( LifeState == LifeState.Alive && IsEnemyPlayer( victim ) )
			{
				if ( LastKillTime < 5f )
				{
					SuccessiveKills++;
				}

				KillStreak++;
			}

			LastKillTime = 0f;
		}

		protected void UpdateAnimatedLegBones( SceneModel model )
		{
			for ( var i = 0; i < model.Model.BoneCount; i++ )
			{
				var boneName = model.Model.GetBoneName( i );

				if ( !LegBonesToKeep.Contains( boneName ) )
				{
					var moveBackBy = 25f;

					if ( boneName == "spine_1" ) moveBackBy = 15f;
					if ( boneName == "spine_0" ) moveBackBy = 10f;
					if ( boneName == "pelvis" ) moveBackBy = 5f;

					var transform = model.GetBoneWorldTransform( i );
					transform.Position += model.Rotation.Backward * moveBackBy;
					transform.Position += model.Rotation.Up * 15f;
					model.SetBoneWorldTransform( i, transform );
				}
			}
		}

		protected virtual void SimulateActiveChild( Entity child )
		{
			if ( Prediction.FirstTime )
			{
				if ( LastActiveChild != child )
				{
					OnActiveChildChanged( LastActiveChild, child );
					LastActiveChild = child;
				}
			}

			if ( !LastActiveChild.IsValid() )
				return;

			if ( LastActiveChild.IsAuthority )
			{
				LastActiveChild.Simulate( Client );
			}
		}

		protected virtual void OnActiveChildChanged( Entity previous, Entity next )
		{
			if ( previous is Weapon previousWeapon )
			{
				previousWeapon?.ActiveEnd( this, previousWeapon.Owner != this );
			}

			if ( next is Weapon nextWeapon )
			{
				nextWeapon?.ActiveStart( this );
			}
		}

		protected virtual float GetFootstepVolume()
		{
			return Velocity.WithZ( 0f ).Length.LerpInverse( 0f, 300f ) * 1f;
		}

		protected virtual void CreateHull()
		{
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16f, -16f, 0f ), new Vector3( 16f, 16f, 72f ) );
			EnableHitboxes = true;
		}

		[ClientRpc]
		protected virtual void OnClientKilled()
		{
			StopJetpackLoop();
			StopSkiLoop();
		}

		[Event.Client.Frame]
		protected virtual void OnFrame()
		{
			if ( AnimatedLegs.IsValid() )
			{
				AnimatedLegs.RenderingEnabled = (LifeState == LifeState.Alive);

				foreach ( var clothing in LegsClothing )
				{
					clothing.SceneObject.RenderingEnabled = AnimatedLegs.RenderingEnabled;
				}

				if ( AnimatedLegs.RenderingEnabled )
				{
					var shouldHideLegs = LegsClothing.Any( c => c.Asset.HideBody.HasFlag( Sandbox.Clothing.BodyGroups.Legs ) );

					AnimatedLegs.SetBodyGroup( "Head", 1 );
					AnimatedLegs.SetBodyGroup( "Hands", 1 );
					AnimatedLegs.SetBodyGroup( "Legs", shouldHideLegs ? 1 : 0 );

					AnimatedLegs.Flags.CastShadows = false;
					AnimatedLegs.Transform = Transform;
					AnimatedLegs.Position += AnimatedLegs.Rotation.Forward * -10f;

					AnimatedLegs.Update( RealTime.Delta );

					foreach ( var clothing in LegsClothing )
					{
						clothing.SceneObject.Flags.CastShadows = false;
						clothing.SceneObject.Update( RealTime.Delta );

						UpdateAnimatedLegBones( clothing.SceneObject );
					}

					UpdateAnimatedLegBones( AnimatedLegs );
				}
			}
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( IsLocalPawn && Controller is MoveController controller )
			{
				var speed = Velocity.Length.Remap( 0f, controller.MaxSpeed, 0f, 1f );
				speed = Math.Min( Easing.EaseIn( speed ) * 60f, 60f );
				SpeedLines.SetPosition( 1, new Vector3( speed, 0f, 0f ) );
			}

			if ( IsLocalPawn )
			{
				UpdateWindLoop();
			}

			UpdateJetpackLoop();
			UpdateSkiLoop();
			UpdateTargetAlpha();

			EnableDrawing = (LifeState == LifeState.Alive);
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( LifeState != LifeState.Alive && RespawnTime.HasValue && RespawnTime.Value )
			{
				UI.RespawnScreen.Hide( To.Single( this ) );
				UI.StationScreen.Show( To.Single( this ), UI.StationScreenMode.Deployment );

				// Respawn bots immediately because they can't select loadouts.
				if ( Client.IsBot ) Respawn();

				RespawnTime = null;
			}

			for ( int i = AssistTrackers.Count - 1; i >= 0; i-- )
			{
				var tracker = AssistTrackers[i];

				if ( tracker.LastDamageTime > 10f )
				{
					AssistTrackers.RemoveAt( i );
				}
			}

			if ( SuccessiveKills > 0 && LastKillTime > 5f )
			{
				SuccessiveKills = 0;
			}

			CheckLowEnergy();
			UpdateHealthRegen();

			Client.SetInt( "tokens", Tokens );
		}

		protected virtual void UpdateTargetAlpha()
		{
			RenderColor = RenderColor.WithAlpha( RenderColor.a.LerpTo( TargetAlpha, Time.Delta * 4f ) );

			foreach ( var child in Children )
			{
				if ( child is ModelEntity model )
				{
					model.RenderColor = model.RenderColor.WithAlpha( model.RenderColor.a.LerpTo( TargetAlpha, Time.Delta * 4f ) );
				}
			}
		}

		protected virtual void UpdateHealthRegen()
		{
			if ( LifeState == LifeState.Dead || !NextRegenTime )
				return;

			if ( Health == MaxHealth )
				return;

			if ( !IsRegenerating )
			{
				IsRegenerating = true;
				PlaySound( "regen.start" );
			}

			Health = Math.Clamp( Health + HealthRegen * Time.Delta, 0f, MaxHealth );
		}

		protected virtual void UpdateJetpackLoop()
		{
			if ( Controller is MoveController controller )
			{
				if ( IsPlayingJetpackLoop )
				{
					if ( !controller.IsJetpacking )
					{
						PlaySound( "jetpack.blast" ).SetVolume( 0.1f );
						StopJetpackLoop();
					}
				}
				else if ( controller.IsJetpacking )
				{
					StartJetpackLoop();
				}
			}
			else if ( IsPlayingJetpackLoop )
			{
				StopJetpackLoop();
			}
		}

		protected virtual void CheckLowEnergy()
		{
			if ( Energy < 5f && PlayLowEnergySound )
			{
				PlaySound( "regen.energylow" );
				PlayLowEnergySound = false;
			}
			else if ( Energy > MaxEnergy * 0.5f && !PlayLowEnergySound )
			{
				PlayLowEnergySound = true;
			}
		}

		protected virtual void StopJetpackLoop()
		{
			IsPlayingJetpackLoop = false;
			JetpackLoop.Stop();
		}

		protected virtual void StartJetpackLoop()
		{
			IsPlayingJetpackLoop = true;
			JetpackLoop.Stop();
			JetpackLoop = PlaySound( "jetpack.fly" );
			PlaySound( "jetpack.blast" );
		}

		protected virtual void UpdateWindLoop()
		{
			if ( Controller is MoveController controller )
			{
				if ( IsPlayingWindLoop )
				{
					if ( GroundEntity.IsValid() || Velocity.Length < controller.MaxSpeed * 0.1f )
					{
						StopWindLoop();
					}
					else
					{
						WindLoop.SetVolume( Velocity.Length.Remap( 0f, controller.MaxSpeed, 0f, 0.8f ) );
					}
				}
				else if ( !GroundEntity.IsValid() && Velocity.Length > controller.MaxSpeed * 0.1f )
				{
					StartWindLoop();
				}
			}
			else if ( IsPlayingWindLoop )
			{
				StopWindLoop();
			}
		}

		protected virtual void UpdateSkiLoop()
		{
			if ( Controller is MoveController controller )
			{
				if ( IsPlayingSkiLoop )
				{
					if ( !controller.IsSkiing || Velocity.Length < controller.MaxSpeed * 0.1f )
					{
						StopSkiLoop();
						PlaySound( "ski.stop" );
					}
					else
					{
						SkiLoop.SetVolume( Velocity.Length.Remap( 0f, controller.MaxSpeed * 0.8f, 0f, 0.8f ) );
					}
				}
				else if ( controller.IsSkiing && Velocity.Length > controller.MaxSpeed * 0.1f )
				{
					StartSkiLoop();
				}
			}
			else if ( IsPlayingSkiLoop )
			{
				StopSkiLoop();
			}
		}

		protected virtual void StopWindLoop()
		{
			IsPlayingWindLoop = false;
			WindLoop.Stop();
		}

		protected virtual void StartWindLoop()
		{
			IsPlayingWindLoop = true;
			WindLoop.Stop();
			WindLoop = PlaySound( "player.windloop" );
			WindLoop.SetVolume( 0.1f );
		}

		protected virtual void StopSkiLoop()
		{
			IsPlayingSkiLoop = false;
			SkiLoop.Stop();
		}

		protected virtual void StartSkiLoop()
		{
			IsPlayingSkiLoop = true;
			SkiLoop.Stop();
			SkiLoop = PlaySound( "ski.loop" );
		}

		protected override void OnDestroy()
		{
			RemoveRagdollEntity();
			StopJetpackLoop();
			StopWindLoop();
			StopSkiLoop();

			if ( IsLocalPawn )
			{
				SpeedLines?.Destroy();
				RadarHud?.Delete();
			}

			base.OnDestroy();
		}
	}
}
