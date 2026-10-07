using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

public sealed class RareEnemySpawnerEvent : GameEvent
{
	public enum ERareEnemyType
	{
		Ryshia,
		Nethershade
	}

	private const int KickstarterEnemyID = 388;

	private const float RareEnemySpawnRate = 0.1f;

	private const string HasEnemySpawnedKeyFormat = "RareEnemySpawned_{0}";

	private readonly ERareEnemyType _enemyType;

	private readonly int _roomID;

	private string HasEnemySpawnedKey => $"RareEnemySpawned_{_roomID}";

	public RareEnemySpawnerEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_doesDrawSpriteAndAppendages = false;
		_roomID = _level.RoomID;
		_enemyType = (ERareEnemyType)objectSpec.Argument;
	}

	public override void Initialize()
	{
		string hasEnemySpawnedKey = HasEnemySpawnedKey;
		if (!_level.GetLevelSaveBool(hasEnemySpawnedKey))
		{
			SpawnEnemy();
			_level.SetLevelSaveBool(hasEnemySpawnedKey, value: true);
		}
		base.Initialize();
	}

	private void SpawnEnemy()
	{
		double num = _level.NextRandomDouble();
		if (num <= 0.10000000149011612)
		{
			_level.ClearAllEnemies();
			ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification(388);
			objectTileSpecification.X = Position.X / 16;
			objectTileSpecification.Y = Position.Y / 16;
			ObjectTileSpecification objectTileSpecification2 = objectTileSpecification;
			switch (_enemyType)
			{
			case ERareEnemyType.Ryshia:
				objectTileSpecification2.Argument = 4;
				break;
			case ERareEnemyType.Nethershade:
				objectTileSpecification2.Argument = 3;
				break;
			}
			_level.PlaceEvent(objectTileSpecification2, shouldInitialize: true);
		}
	}
}
