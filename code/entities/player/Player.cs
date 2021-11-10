using Gamelib.Extensions;
using Sandbox;
using Sandbox.Joints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Hover
{
	public partial class Player : Sandbox.Player
	{
		public Dictionary<string,List<WeaponUpgrade>> WeaponUpgrades { get; private set; }
		public ProjectileSimulator Projectiles { get; private set; }
		public HashSet<Type> LoadoutUpgrades { get; private set; }
		public List<Award> EarnedAwards { get; private set; }
		public TimeSince LastKillTime { get; private set; }
		public int SuccessiveKills { get; private set; }

		[ServerCmd]
		public static void BuyWeaponUpgrade( string weaponName, string upgradeName )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				var upgradeType = Library.Get<WeaponUpgrade>( upgradeName );
				if ( upgradeType == null ) return;

				foreach ( var weapon in player.Children.OfType<Weapon>() )
				{
					if ( weapon.Name == weaponName && weapon.Upgrades != null )
					{
						if ( weapon.Upgrades.Contains( upgradeType ) )
						{
							var upgrade = Library.Create<WeaponUpgrade>( upgradeType );

							if ( player.HasTokens( upgrade.TokenCost) )
							{
								player.GiveWeaponUpgrade( weapon, upgrade );
								player.TakeTokens( upgrade.TokenCost );

								upgrade.Apply( player, weapon );

								player.Restock();
							}

							return;
						}
					}
				}
			}
		}

		[ServerCmd]
		public static void BuyLoadoutUpgrade( string loadoutName, string weapons )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				var loadoutType = Library.Get<BaseLoadout>( loadoutName );

				if ( loadoutType != null )
				{
					var loadout = Library.Create<BaseLoadout>( loadoutType );
					if ( loadout == null ) return;

					if ( player.HasTokens( loadout.UpgradeCost ) )
					{
						player.GiveLoadoutUpgrade( loadoutType );
						player.TakeTokens( loadout.UpgradeCost );
						player.GiveLoadout( loadout );

						loadout.UpdateWeapons( weapons.Split( ',' ) );
						loadout.Respawn( player );
						loadout.Supply( player );
					}
				}
			}
		}

		[ServerCmd]
		public static void BuyLoadout( string loadoutName, string weapons )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				var loadoutType = Library.Get<BaseLoadout>( loadoutName );

				if ( loadoutType != null )
				{
					var loadout = Library.Create<BaseLoadout>( loadoutType );
					if ( loadout == null ) return;

					if ( player.HasTokens( loadout.TokenCost ) )
					{
						loadout.UpdateWeapons( weapons.Split( ',' ) );

						player.TakeTokens( loadout.TokenCost );
						player.GiveLoadout( loadout );

						if ( player.LifeState == LifeState.Alive )
						{
							loadout.Respawn( player );
							loadout.Supply( player );
						}
						else
						{
							player.Respawn();
						}
					}
				}
			}
		}

		private class AssistTracker
		{
			public TimeSince LastDamageTime { get; set; }
			public Player Attacker { get; set; }
			public float TotalDamage { get; set; }
		}

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

		public RealTimeUntil? RespawnTime { get; set; }
		public DamageInfo LastDamageInfo { get; set; }
		public Player LastKiller { get; set; }

		private List<AssistTracker> AssistTrackers { get; set; }
		private Rotation LastCameraRotation { get; set; }
		private Particles SpeedLines { get; set; }
		private Nameplate Nameplate { get; set; }
		private Radar RadarHud { get; set; }
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

		public Player()
		{
			LoadoutUpgrades = new();
			WeaponUpgrades = new();
			AssistTrackers = new();
			EarnedAwards = new();
			Projectiles = new( this );
			EnableTouch = true;
			Inventory = new Inventory( this );
			Animator = new PlayerAnimator();
		}

		public bool IsSpectator
		{
			get => (Camera is SpectateCamera);
		}

		public void Reset()
		{
			Client.SetInt( "captures", 0 );
			Client.SetInt( "deaths", 0 );
			Client.SetInt( "kills", 0 );

			LoadoutUpgrades.Clear();
			WeaponUpgrades.Clear();
			Projectiles.Clear();
			EarnedAwards.Clear();
			LastDamageInfo = default;
			LastKiller = null;
			Tokens = 0;
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
			var weaponName = weapon.Config.Name;

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

		public List<WeaponUpgrade> GetWeaponUpgrades( Weapon weapon )
		{
			if ( WeaponUpgrades.TryGetValue( weapon.Config.Name, out var upgrades ) )
			{
				return upgrades;
			}

			return null;
		}

		public void GiveWeaponUpgrade( Weapon weapon, WeaponUpgrade upgrade )
		{
			var weaponName = weapon.Config.Name;

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

		[ClientRpc]
		public void GiveWeaponUpgrade( string weaponName, string typeName )
		{
			var upgrade = Library.Create<WeaponUpgrade>( typeName );

			if ( WeaponUpgrades.TryGetValue( weaponName, out var upgrades ) )
			{
				upgrades.Add( upgrade );
				StationScreen.Refresh();
				return;
			}

			upgrades = new List<WeaponUpgrade>();
			WeaponUpgrades[weaponName] = upgrades;
			upgrades.Add( upgrade );
			StationScreen.Refresh();
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
			var type = Library.Get<BaseLoadout>( typeName );

			if ( type != null )
			{
				LoadoutUpgrades.Add( type );
				StationScreen.Refresh();
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
		public void ShowAward( string name, Player awardee )
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

		public Player GetBestAssist( Entity attacker )
		{
			var minDamageTarget = MaxHealth * 0.3f;
			var assister = (Player)null;
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
			Camera = new SpectateCamera();
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

		public override void Spawn()
		{
			LagCompensation = true;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			if ( IsLocalPawn )
			{
				SpeedLines = Particles.Create( "particles/player/speed_lines.vpcf" );
				RadarHud = Local.Hud.AddChild<Radar>();
			}
			else
			{
				Nameplate = new Nameplate( this );
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

		public override void Respawn()
		{
			base.Respawn();

			RemoveRagdollEntity();
			ClearAssistTrackers();

			SuccessiveKills = 0;
			TimeSinceSpawn = 0f;
			KillStreak = 0;

			Rounds.Current?.OnPlayerSpawn( this );

			ClientRespawn();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			var attacker = LastAttacker;

			if ( attacker.IsValid() )
			{
				if ( attacker is Player killer )
				{
					killer.OnKillPlayer( this, LastDamageInfo );
				}

				Rounds.Current?.OnPlayerKilled( this, attacker, LastDamageInfo );
			}
			else
			{
				Rounds.Current?.OnPlayerKilled( this, null, LastDamageInfo );
			}


			BecomeRagdollOnClient( LastDamageInfo.Force, GetHitboxBone( LastDamageInfo.HitboxIndex ) );
			Inventory.DeleteContents();

			if ( LastDamageInfo.Flags.HasFlag( DamageFlags.Fall ) )
			{
				PlaySound( "player.falldie" );
			}

			var bloodExplosion = Particles.Create( "particles/blood/explosion_blood/explosion_blood.vpcf", Position );
			bloodExplosion.SetForward( 0, LastDamageInfo.Force.Normal );

			StationScreen.Hide( To.Single( this ) );

			PlaySound( $"grunt{Rand.Int( 1, 4 )}" );

			SuccessiveKills = 0;
			KillStreak = 0;

			OnClientKilled();
		}

		public override void BuildInput( InputBuilder input )
		{
			var stationScreen = StationScreen.Instance;

			if ( stationScreen.IsOpen )
			{
				if ( stationScreen.Mode == StationScreenMode.Station && input.Released( InputButton.Use ) )
				{
					StationScreen.Hide();
				}

				input.StopProcessing = true;
				input.ClearButtons();
				input.Clear();

				return;
			}

			base.BuildInput( input );
		}

		public override void Simulate( Client client )
		{
			Projectiles.Simulate();

			SimulateActiveChild( client, ActiveChild );

			var targetWeapon = Input.ActiveChild as Weapon;

			if ( targetWeapon != null && ActiveChild != targetWeapon )
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
				var trace = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 20000f )
					.Ignore( this )
					.Run();

				foreach ( var player in All.OfType<Player>() )
				{
					if ( !IsEnemyPlayer( player ) )
						continue;

					if ( player.Position.DistanceToLine( trace.StartPos, trace.EndPos, out var _ ) > 200f )
						continue;

					var visibleTrace = Trace.Ray( EyePos, EyePos + (player.WorldSpaceBounds.Center - EyePos).Normal * 20000f )
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

			if ( Input.Released( InputButton.View ) )
			{
				if ( Camera is FirstPersonCamera )
					Camera = new ThirdPersonCamera();
				else
					Camera = new FirstPersonCamera();
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
				var station = Physics.GetEntitiesInSphere( Position, 50f )
					.OfType<StationAsset>()
					.FirstOrDefault();

				if ( station != null && station.CanPlayerUse( this ) )
				{
					using ( Prediction.Off() )
					{
						station.ShowUseEffects();
						StationScreen.Show( To.Single( this ), StationScreenMode.Station );
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

			var controller = GetActiveController();
			controller?.Simulate( client, this, GetActiveAnimator() );
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );

			if ( LastCameraRotation == Rotation.Identity )
				LastCameraRotation = CurrentView.Rotation;

			var angleDiff = Rotation.Difference( LastCameraRotation, CurrentView.Rotation );
			var angleDiffDegrees = angleDiff.Angle();
			var allowance = 20.0f;

			if ( angleDiffDegrees > allowance )
			{
				LastCameraRotation = Rotation.Lerp( LastCameraRotation, CurrentView.Rotation, 1.0f - (allowance / angleDiffDegrees) );
			}
			
			AddCameraEffects( ref setup );
		}

		private void ClearAssistTrackers()
		{
			AssistTrackers.Clear();
		}

		private AssistTracker GetAssistTracker( Player attacker )
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

		private void AddAssistDamage( Player attacker, DamageInfo info )
		{
			var tracker = GetAssistTracker( attacker );

			if ( tracker.LastDamageTime > 10f )
			{
				tracker.TotalDamage = 0f;
			}

			tracker.LastDamageTime = 0f;
			tracker.TotalDamage += info.Damage;
		}

		private void AddCameraEffects( ref CameraSetup setup )
		{
			if ( Controller is not MoveController controller )
				return;

			var forwardSpeed = Velocity.Normal.Dot( setup.Rotation.Forward );
			var speed = Velocity.Length.LerpInverse( 0, controller.MaxSpeed );
			var left = setup.Rotation.Left;
			var up = setup.Rotation.Up;

			if ( GroundEntity != null )
			{
				WalkBob += Time.Delta * 25.0f * speed;
			}

			setup.Position += up * MathF.Sin( WalkBob ) * speed * 2;
			setup.Position += left * MathF.Sin( WalkBob * 0.6f ) * speed * 1;

			speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

			FOV = FOV.LerpTo( speed * 30 * MathF.Abs( forwardSpeed ), Time.Delta * 2.0f );

			setup.FieldOfView += FOV;
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( info.HitboxIndex == 0 && !info.Flags.HasFlag( DamageFlags.Blast ) )
			{
				info.Damage *= 2.0f;
			}

			if ( Controller is MoveController controller )
			{
				controller.Impulse += info.Force;
			}

			if ( info.Attacker is Player attacker && attacker != this )
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

				if ( info.Flags.HasFlag( DamageFlags.Blunt ) )
				{
					var dotDirection = EyeRot.Forward.Dot( attacker.EyeRot.Forward );

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

			TookDamage( To.Single( this ), fromPosition, info.Damage, info.Flags );

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

			base.TakeDamage( info );
		}

		[ClientRpc]
		public void ShowFloatingDamage( float damage, Vector3 position )
		{
			// Don't show damage that happened to us.
			if ( IsLocalPawn ) return;

			var panel = FloatingDamage.Rent();

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
			HitIndicator.Current?.OnHit( position, amount );
		}

		[ClientRpc]
		public void TookDamage( Vector3 position, float amount, DamageFlags flags )
		{
			if ( flags.HasFlag( DamageFlags.Fall ) )
			{
				_ = new Sandbox.ScreenShake.Perlin( 2f, 1f, amount.Remap( 0f, MaxHealth, 0f, 10f ), 0.8f );
			}

			DamageIndicator.Current?.OnHit( position );
		}

		public virtual bool IsEnemyPlayer( Player other )
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

		public virtual void OnKillPlayer( Player victim, DamageInfo damageInfo )
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

		[ClientRpc]
		protected virtual void OnClientKilled()
		{
			StopJetpackLoop();
			StopSkiLoop();
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
				Respawn();
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
					if ( controller.GroundEntity.IsValid() || Velocity.Length < controller.MaxSpeed * 0.1f )
					{
						StopWindLoop();
					}
					else
					{
						WindLoop.SetVolume( Velocity.Length.Remap( 0f, controller.MaxSpeed, 0f, 0.8f ) );
					}
				}
				else if ( !controller.GroundEntity.IsValid() && Velocity.Length > controller.MaxSpeed * 0.1f )
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

		protected override void UseFail()
		{
			
		}

		protected override Entity FindUsable()
		{
			var trace = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 150f )
				.HitLayer( CollisionLayer.Water, true )
				.Ignore( this )
				.Run();

			if ( !IsValidUseEntity( trace.Entity ) )
			{
				trace = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 150f )
				.HitLayer( CollisionLayer.Water, true )
				.Radius( 2 )
				.Ignore( this )
				.Run();
			}

			if ( !IsValidUseEntity( trace.Entity ) )
				return null;

			return trace.Entity;
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
