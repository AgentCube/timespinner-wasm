using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Treasure;

internal abstract class BaseJournalEntryEvent : GameEvent
{
	private const float TimeToFade = 0.25f;

	private readonly EInventoryJournalType _journalType;

	private bool _isFading;

	private float _fadeTimer;

	private float _fadePercentage;

	internal bool IsMemory => _journalType < EInventoryJournalType.Letter0;

	internal bool IsFading => _isFading;

	internal int MoneyGiven { get; set; }

	internal EInventoryJournalType JournalType => _journalType;

	internal float FadePercentage => _fadePercentage;

	internal BaseJournalEntryEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_journalType = (EInventoryJournalType)(objectSpec?.Argument ?? 0);
		Bbox = new Rectangle(0, 0, 16, 16);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = false;
		_doesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		base.IsTriggerableByMonsters = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _isFading)
		{
			_fadeTimer += delta;
			if (_fadeTimer > 0.25f)
			{
				RemoveAfterGetting();
			}
			else
			{
				_fadePercentage = _fadeTimer / 0.25f;
			}
		}
		base.Update(delta);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (_level.GameSave.Inventory.JournalCollection.Inventory.ContainsKey((int)_journalType))
		{
			_level.RequestRemoveObject(this);
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (who != null && !_isFading)
		{
			GetJournalEntry();
		}
		return true;
	}

	internal virtual bool OnGetJournalEntry()
	{
		return true;
	}

	private void GetJournalEntry()
	{
		_level.PlayCue(ESFX.ItemGetGeneral, Bbox.Center);
		if (OnGetJournalEntry())
		{
			_level.GameSave.UnlockJournal(_journalType);
			_level.RequestItemGetPopup(_journalType);
			_level.GameSave.FeatsManager.UnlockJournalAchievement(_level.GameSave.Inventory.JournalCollection, _journalType);
		}
		_isFading = true;
		if (MoneyGiven > 0 && _level.MainHero != null)
		{
			_level.MainHero.GetPowerup(EItemType.Money, MoneyGiven);
		}
	}

	private void RemoveAfterGetting()
	{
		_level.RequestRemoveObject(this);
	}

	public static GameEvent FromSpecification(Level level, Point tilePosition, int newObjectID, ObjectTileSpecification objectTileSpec)
	{
		int num = objectTileSpec?.Argument ?? 0;
		if (num < 32)
		{
			return new JournalMemoryEvent(level, tilePosition, newObjectID, objectTileSpec);
		}
		if (num < 64)
		{
			return new JournalLetterEvent(level, tilePosition, newObjectID, objectTileSpec);
		}
		return new JournalComputerEvent(level, tilePosition, newObjectID, objectTileSpec);
	}
}
