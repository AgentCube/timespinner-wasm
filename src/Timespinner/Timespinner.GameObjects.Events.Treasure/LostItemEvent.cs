using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Items;

namespace Timespinner.GameObjects.Events.Treasure;

internal sealed class LostItemEvent : GameEvent
{
	private const string SaveKey = "Item{0},{1},{2},{3},{4}";

	private readonly ObjectTileSpecification _objectSpec;

	private readonly ItemDropPickup _itemDrop;

	private bool _hasItemBeenFound;

	public LostItemEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_objectSpec = objectSpec;
		Bbox = new Rectangle(Position.X, Position.Y, 16, 16);
		base.IsAffectedByTime = false;
		bool isSpeedrunBActive = _level.GameSave.IsSpeedrunBActive;
		if (_level.GameSave.GetSaveBool(GetSaveKey()) || isSpeedrunBActive)
		{
			_hasItemBeenFound = true;
			_level.RequestRemoveObject(this);
			return;
		}
		BestiaryItemDropSpecification bestiaryItemDropSpecification = new BestiaryItemDropSpecification();
		Point point = TreasureChestEvent.DeMuxArgument(objectSpec.Argument);
		switch (point.X)
		{
		case 1:
			bestiaryItemDropSpecification.Category = 1;
			break;
		case 2:
			bestiaryItemDropSpecification.Category = 6;
			break;
		case 3:
			bestiaryItemDropSpecification.Category = 4;
			break;
		}
		bestiaryItemDropSpecification.Item = point.Y;
		_itemDrop = new ItemDropPickup(bestiaryItemDropSpecification, _level, Position, -1);
		_level.AddItem(_itemDrop);
	}

	private string GetSaveKey()
	{
		return $"Item{_level.ID},{_level.RoomID},{_objectSpec.Argument},{_objectSpec.X},{_objectSpec.Y}";
	}

	public override void Update(float delta)
	{
		if (_itemDrop != null && !_hasItemBeenFound && _itemDrop.IsFound)
		{
			_hasItemBeenFound = true;
			_level.GameSave.SetValue(GetSaveKey(), value: true);
			Kill();
		}
		base.Update(delta);
	}
}
