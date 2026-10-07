using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class GyreSpawnerEvent : GameEvent
{
	private const int FloorHeight16 = 11;

	private const int FlyingHeight16 = 6;

	private const int CeilingHeight16 = 2;

	private const int SpawnLeft16 = 10;

	private const int SpawnRight16 = 40;

	private const int SpawnWidth16 = 30;

	private const int RareSpawnX = 25;

	private const int EnemyTypeMin = 0;

	private const int EnemyTypeMax = 47;

	private const int MinEnemyTypeVariances = 1;

	private const int MaxEnemyTypeVariance = 4;

	private const int MinEnemySpawns = 6;

	private const int MaxEnemySpawns = 12;

	private const float RoomDoorOpenThreshold = 30f;

	private const string RoomClearedKeyFormat = "GyreRoomCleared_{0}";

	private const string RareEnemyDeadFormat = "GyreRareDead_{0}";

	private readonly int _roomID;

	private readonly int _roomSeed;

	private readonly Random _randomizer;

	private readonly List<ObjectTileSpecification> _enemyVariantSpecifications = new List<ObjectTileSpecification>();

	private readonly List<Monster> _spawnedEnemies = new List<Monster>();

	private bool _hasOpenedDoors;

	private bool _isRoomCleared;

	private bool _hasSpawnedRareEnemy;

	private bool _isThePlayerComingFromTheLeft;

	private int _rareEnemyArgument;

	private float _roomTimer;

	private Monster _rareEnemy;

	private string RoomClearedKey => $"GyreRoomCleared_{_roomID}";

	public GyreSpawnerEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_roomID = _level.RoomID;
		int levelSaveInt = _level.GetLevelSaveInt("GyreDungeonSeed");
		_roomSeed = levelSaveInt + _roomID;
		_randomizer = new Random(_roomSeed);
		_doesDrawSpriteAndAppendages = false;
		_rareEnemyArgument = -1;
	}

	public override void Initialize()
	{
		_isRoomCleared = _level.GetLevelSaveBool(RoomClearedKey);
		if (!_isRoomCleared)
		{
			_isThePlayerComingFromTheLeft = _level.GetNearestProtagonistPosition(Position).X < Position.X;
			SpawnEnemies();
		}
		else
		{
			OpenDoors(isInstant: true);
		}
		base.Initialize();
	}

	public override void Update(float delta)
	{
		if (!_isRoomCleared)
		{
			bool flag = true;
			foreach (Monster spawnedEnemy in _spawnedEnemies)
			{
				if (spawnedEnemy.HP > 0 && !spawnedEnemy.IsDead)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				_isRoomCleared = true;
				_level.SetLevelSaveBool(RoomClearedKey, value: true);
				OpenDoors(isInstant: false);
			}
			else if (!_level.IsTimeFrozen && !_hasOpenedDoors)
			{
				_roomTimer += delta;
				if (_roomTimer >= 30f)
				{
					OpenDoors(isInstant: false);
				}
			}
		}
		base.Update(delta);
	}

	private void SpawnEnemies()
	{
		int num = RandomBetween(1, 4, _randomizer);
		PopulateVariants(num);
		int num2 = RandomBetween(6, 12, _randomizer);
		if (num2 <= 0)
		{
			return;
		}
		int num3 = (int)Math.Floor(30f / (float)num2);
		if (num3 < 1)
		{
			num3 = 1;
		}
		int num4 = 10;
		for (int i = 0; i < num2; i++)
		{
			int variant = i % num;
			Monster monster = PlaceEnemyInLevel(variant, num4);
			if (monster != null)
			{
				_spawnedEnemies.Add(monster);
			}
			num4 += num3;
		}
		DoEntryScript();
	}

	private void PopulateVariants(int totalVariants)
	{
		for (int i = 0; i < totalVariants; i++)
		{
			ObjectTileSpecification objectTileSpecification = PickEnemy(_randomizer, _isThePlayerComingFromTheLeft, _rareEnemyArgument == -1, canBeCeiling: true);
			if (GetIsEnemyRare(objectTileSpecification))
			{
				_rareEnemyArgument = objectTileSpecification.Argument;
			}
			_enemyVariantSpecifications.Add(objectTileSpecification);
		}
	}

	internal static ObjectTileSpecification PickEnemy(Random randomizer, bool shouldFaceLeft, bool canBeRare, bool canBeCeiling)
	{
		ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification();
		objectTileSpecification.Category = EObjectTileCategory.Enemy;
		objectTileSpecification.Layer = ETileLayerType.Objects;
		ObjectTileSpecification objectTileSpecification2 = objectTileSpecification;
		bool isFlippedVertically = false;
		int num = 0;
		int y = 11;
		EEnemyTileType eEnemyTileType = (EEnemyTileType)RandomBetween(0, 47, randomizer);
		switch (eEnemyTileType)
		{
		case EEnemyTileType.FlyingCheveux:
		case EEnemyTileType.KeepDemon:
		case EEnemyTileType.ForestMoth:
		case EEnemyTileType.TowerPlasmaPod:
		case EEnemyTileType.LakeFly:
			y = 6;
			break;
		case EEnemyTileType.CavesSlime:
		case EEnemyTileType.CeilingStar:
		case EEnemyTileType.FleshSpider:
		case EEnemyTileType.ForestPlantBat:
		case EEnemyTileType.CavesSporeVine:
			if (canBeCeiling)
			{
				y = 2;
				isFlippedVertically = true;
			}
			else
			{
				eEnemyTileType = EEnemyTileType.CheveuxTank;
			}
			break;
		default:
			eEnemyTileType = EEnemyTileType.KickstarterFoe;
			break;
		case EEnemyTileType.CheveuxTank:
		case EEnemyTileType.RedCheveux:
		case EEnemyTileType.CavesCopperWyvern:
		case EEnemyTileType.CastleShieldKnight:
		case EEnemyTileType.CastleArcher:
		case EEnemyTileType.WormFlower:
		case EEnemyTileType.WormFlowerWalker:
		case EEnemyTileType.DiscStatue:
		case EEnemyTileType.CitySecurityGuard:
		case EEnemyTileType.ForestBabyCheveux:
		case EEnemyTileType.ForestRodent:
		case EEnemyTileType.ForestWormFlower:
		case EEnemyTileType.CavesMushroomTower:
		case EEnemyTileType.CastleLargeSoldier:
		case EEnemyTileType.KeepWarCheveux:
		case EEnemyTileType.KeepAristocrat:
		case EEnemyTileType.TowerRoyalGuard:
		case EEnemyTileType.LakeBirdEgg:
		case EEnemyTileType.LakeCheveux:
		case EEnemyTileType.FortressKnight:
		case EEnemyTileType.FortressGunner:
		case EEnemyTileType.LabChild:
		case EEnemyTileType.FortressLargeSoldier:
			break;
		}
		int num2 = 0;
		switch (eEnemyTileType)
		{
		case EEnemyTileType.CavesSlime:
		case EEnemyTileType.CavesCopperWyvern:
		case EEnemyTileType.KeepDemon:
		case EEnemyTileType.FleshSpider:
		case EEnemyTileType.ForestMoth:
		case EEnemyTileType.CavesMushroomTower:
		case EEnemyTileType.CavesSporeVine:
		case EEnemyTileType.TowerRoyalGuard:
		case EEnemyTileType.LakeCheveux:
			num2 = 1;
			break;
		case EEnemyTileType.KeepAristocrat:
			num2 = 2;
			break;
		case EEnemyTileType.TempleFoe:
			num2 = 3;
			break;
		case EEnemyTileType.KickstarterFoe:
			num2 = 5;
			break;
		}
		if (num2 > 0)
		{
			num = RandomBetween(0, num2, randomizer);
		}
		if (eEnemyTileType == EEnemyTileType.KickstarterFoe && num > 0)
		{
			y = 6;
		}
		if (GetIsEnemyRare(eEnemyTileType, num) && !canBeRare)
		{
			eEnemyTileType = EEnemyTileType.CheveuxTank;
			num = 0;
			y = 11;
		}
		objectTileSpecification2.ObjectID = (int)eEnemyTileType;
		objectTileSpecification2.Y = y;
		objectTileSpecification2.IsFlippedVertically = isFlippedVertically;
		objectTileSpecification2.Argument = num;
		if (!shouldFaceLeft)
		{
			objectTileSpecification2.IsFlippedHorizontally = true;
		}
		return objectTileSpecification2;
	}

	private Monster PlaceEnemyInLevel(int variant, int spawnX)
	{
		Monster monster = null;
		ObjectTileSpecification objectTileSpecification = _enemyVariantSpecifications[variant];
		bool isEnemyRare = GetIsEnemyRare((EEnemyTileType)objectTileSpecification.ObjectID, objectTileSpecification.Argument);
		if (!isEnemyRare || (!_hasSpawnedRareEnemy && !_level.GetLevelSaveBool($"GyreRareDead_{_roomID}")))
		{
			objectTileSpecification.X = (isEnemyRare ? 25 : spawnX);
			Animate animate = _level.PlaceEvent(objectTileSpecification, shouldInitialize: true);
			if (animate != null && animate.BaseType == EGameObjectBaseType.Monster)
			{
				monster = animate as Monster;
			}
			if (isEnemyRare)
			{
				_hasSpawnedRareEnemy = true;
				_rareEnemy = monster;
			}
		}
		return monster;
	}

	private void DoEntryScript()
	{
		OpenDoors(isInstant: true);
		_level.LockAllBossDoors(0.5f);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Run, 0f, 0.15f, new Vector4(_isThePlayerComingFromTheLeft ? 1 : (-1), 0f, 0f, 0f))
		{
			DoesBlockQueue = true
		});
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.55f, Vector4.Zero));
	}

	private void OpenDoors(bool isInstant)
	{
		if (!_hasOpenedDoors || isInstant)
		{
			if (!isInstant)
			{
				_hasOpenedDoors = true;
			}
			_level.OpenAllBossDoors((!isInstant) ? 1 : (-1));
		}
	}

	private static int RandomBetween(int min, int max, Random randomizer)
	{
		return (int)Math.Round(randomizer.NextDouble() * (double)(max - min)) + min;
	}

	private bool GetIsEnemyRare(ObjectTileSpecification objectSpec)
	{
		EEnemyTileType enemyType = objectSpec.GetEnemyType();
		return GetIsEnemyRare(enemyType, objectSpec.Argument);
	}

	private static bool GetIsEnemyRare(EEnemyTileType enemyType, int argument)
	{
		bool result = false;
		if (enemyType == EEnemyTileType.KickstarterFoe)
		{
			result = argument != 1;
		}
		return result;
	}

	public override void SilentKill()
	{
		if (!_isRoomCleared && _hasOpenedDoors && _rareEnemy != null && _rareEnemy.HP <= 0)
		{
			_level.SetLevelSaveBool($"GyreRareDead_{_roomID}", value: true);
		}
	}
}
