using System;

namespace Timespinner.Core.Specifications;

[Serializable]
public class ObjectTileSpecification : TileSpecification
{
	public EObjectTileCategory Category { get; set; }

	public int ObjectID { get; set; }

	public bool DoesHaveArgument => base.Argument != 0;

	public ObjectTileSpecification()
	{
	}

	public ObjectTileSpecification(TileSpecification toCopy)
	{
		base.Layer = toCopy.Layer;
		base.ID = toCopy.ID;
		base.X = toCopy.X;
		base.Y = toCopy.Y;
		base.IsFlippedHorizontally = toCopy.IsFlippedHorizontally;
		base.IsFlippedVertically = toCopy.IsFlippedVertically;
		base.Switches = toCopy.Switches;
	}

	public ObjectTileSpecification(int tileID)
	{
		base.ID = tileID;
		SetObjectIDAndCategoryFromTileID();
	}

	public static ObjectTileSpecification FromTileSpecification(TileSpecification tileSpec)
	{
		ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification(tileSpec.ID);
		objectTileSpecification.X = tileSpec.X;
		objectTileSpecification.Y = tileSpec.Y;
		objectTileSpecification.Layer = tileSpec.Layer;
		objectTileSpecification.Argument = tileSpec.Argument;
		objectTileSpecification.IsFlippedHorizontally = tileSpec.IsFlippedHorizontally;
		objectTileSpecification.IsFlippedVertically = tileSpec.IsFlippedVertically;
		objectTileSpecification.Switches = tileSpec.Switches;
		return objectTileSpecification;
	}

	public EEventTileType GetEventType()
	{
		return (EEventTileType)ObjectID;
	}

	public EEnemyTileType GetEnemyType()
	{
		return (EEnemyTileType)ObjectID;
	}

	public EItemTileType GetItemType()
	{
		return (EItemTileType)ObjectID;
	}

	public override TileSpecification Duplicate()
	{
		ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification();
		objectTileSpecification.Layer = base.Layer;
		objectTileSpecification.X = base.X;
		objectTileSpecification.Y = base.Y;
		objectTileSpecification.ID = base.ID;
		objectTileSpecification.Category = Category;
		objectTileSpecification.ObjectID = ObjectID;
		objectTileSpecification.Argument = base.Argument;
		objectTileSpecification.IsFlippedHorizontally = base.IsFlippedHorizontally;
		objectTileSpecification.IsFlippedVertically = base.IsFlippedVertically;
		ObjectTileSpecification objectTileSpecification2 = objectTileSpecification;
		foreach (SwitchSpecification @switch in base.Switches)
		{
			objectTileSpecification2.Switches.Add(@switch.Duplicate());
		}
		return objectTileSpecification2;
	}

	public int GetIconIndex()
	{
		int result = 0;
		switch (Category)
		{
		case EObjectTileCategory.Enemy:
			result = ObjectID;
			break;
		case EObjectTileCategory.Item:
			result = ObjectID + 64;
			break;
		case EObjectTileCategory.Event:
			result = ObjectID + 64 + 7;
			break;
		}
		return result;
	}

	public bool IsCheckpoint()
	{
		if (Category == EObjectTileCategory.Event)
		{
			return GetEventType() == EEventTileType.Checkpoint;
		}
		return false;
	}

	public bool IsTransition()
	{
		if (Category == EObjectTileCategory.Event)
		{
			return GetEventType() == EEventTileType.TransitionWarpEvent;
		}
		return false;
	}

	public bool IsBreakableWall()
	{
		if (Category == EObjectTileCategory.Event)
		{
			return GetEventType() == EEventTileType.BreakableWall;
		}
		return false;
	}

	public bool IsTimespinner()
	{
		bool flag = Category == EObjectTileCategory.Event && GetEventType() == EEventTileType.TheTimespinner;
		if (!flag && Category == EObjectTileCategory.Enemy)
		{
			int enemyType = (int)GetEnemyType();
			flag = enemyType == 54;
		}
		return flag;
	}

	public bool IsDoor()
	{
		bool result = false;
		if (Category == EObjectTileCategory.Event)
		{
			EEventTileType eventType = GetEventType();
			if (eventType == EEventTileType.WestTeleport || eventType == EEventTileType.NorthTeleport || eventType == EEventTileType.EastTeleport || eventType == EEventTileType.SouthTeleport || eventType == EEventTileType.Doorway)
			{
				result = true;
			}
		}
		return result;
	}

	public bool IsBoss()
	{
		bool result = false;
		if (Category == EObjectTileCategory.Enemy)
		{
			int enemyType = (int)GetEnemyType();
			if (enemyType >= 48 && enemyType <= 57)
			{
				result = true;
			}
		}
		return result;
	}

	private void SetObjectIDAndCategoryFromTileID()
	{
		if (base.ID >= 384)
		{
			if (base.ID < 448)
			{
				ObjectID = base.ID - 384;
				Category = EObjectTileCategory.Enemy;
			}
			else if (base.ID < 455)
			{
				ObjectID = base.ID - 448;
				Category = EObjectTileCategory.Item;
			}
			else
			{
				ObjectID = base.ID - 455;
				Category = EObjectTileCategory.Event;
			}
		}
	}

	public bool DoesUseArgumentForKey()
	{
		bool result = false;
		switch (Category)
		{
		case EObjectTileCategory.Enemy:
		{
			EEnemyTileType enemyType = GetEnemyType();
			result = DoesUseArgumentForKey(enemyType);
			break;
		}
		case EObjectTileCategory.Event:
			switch (GetEventType())
			{
			case EEventTileType.Lantern:
				result = true;
				break;
			case EEventTileType.Elevator:
				result = true;
				break;
			case EEventTileType.EnvironmentPrefab:
				result = true;
				break;
			}
			break;
		}
		return result;
	}

	public static bool DoesUseArgumentForKey(EEnemyTileType enemy)
	{
		bool result = false;
		switch (enemy)
		{
		case EEnemyTileType.FleshSpider:
			result = true;
			break;
		case EEnemyTileType.ForestMoth:
			result = true;
			break;
		case EEnemyTileType.LakeCheveux:
			result = true;
			break;
		case EEnemyTileType.CastleShieldKnight:
			result = true;
			break;
		case EEnemyTileType.KeepWarCheveux:
			result = true;
			break;
		case EEnemyTileType.KickstarterFoe:
			result = true;
			break;
		case EEnemyTileType.CavesCopperWyvern:
			result = true;
			break;
		case EEnemyTileType.CavesSiren:
			result = true;
			break;
		case EEnemyTileType.CavesMushroomTower:
			result = true;
			break;
		case EEnemyTileType.CavesSporeVine:
			result = true;
			break;
		case EEnemyTileType.CavesSnail:
			result = true;
			break;
		case EEnemyTileType.CavesSlime:
			result = true;
			break;
		case EEnemyTileType.KeepAristocrat:
			result = true;
			break;
		case EEnemyTileType.KeepDemon:
			result = true;
			break;
		case EEnemyTileType.TowerRoyalGuard:
			result = true;
			break;
		case EEnemyTileType.TempleFoe:
			result = true;
			break;
		case EEnemyTileType.IncubusBoss:
			result = true;
			break;
		case EEnemyTileType.MawBoss:
			result = true;
			break;
		case EEnemyTileType.EmperorBoss:
			result = true;
			break;
		case EEnemyTileType.XarionBoss:
			result = true;
			break;
		}
		return result;
	}
}
