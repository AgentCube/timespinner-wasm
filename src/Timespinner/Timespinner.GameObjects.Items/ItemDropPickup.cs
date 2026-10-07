using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Items;

internal sealed class ItemDropPickup : Item
{
	private const float FlashFrequency = 18f;

	private const float FlashFlashTime = 1f;

	private const float FlashPauseTime = 2f;

	private const float TotalFlashTime = 3f;

	private readonly EInventoryCategoryType _category;

	private readonly EInventoryUseItemType _useItemType;

	private readonly EInventoryEquipmentType _equipmentType;

	private readonly EInventoryRelicType _relicType;

	private readonly BestiaryItemDropSpecification _itemData;

	private float _flashTimer;

	internal bool IsFound { get; private set; }

	public ItemDropPickup(BestiaryItemDropSpecification itemDropData, Level inLevel, Point inPosition, int inID)
		: base(inLevel, inPosition, EItemType.ItemDrop, 0f, inID)
	{
		_sprite = _level.GCM.SpMenuIcons;
		_itemData = itemDropData;
		_category = (EInventoryCategoryType)_itemData.Category;
		EInventoryItemIcon iconFromItem;
		switch (_category)
		{
		case EInventoryCategoryType.Relic:
			_relicType = (EInventoryRelicType)_itemData.Item;
			iconFromItem = InventoryItem.GetIconFromItem(_relicType);
			break;
		case EInventoryCategoryType.UseItem:
			_useItemType = (EInventoryUseItemType)_itemData.Item;
			iconFromItem = InventoryItem.GetIconFromItem(_useItemType);
			break;
		default:
			_equipmentType = (EInventoryEquipmentType)_itemData.Item;
			iconFromItem = InventoryItem.GetIconFromItem(_equipmentType);
			break;
		}
		ChangeAnimation((int)(iconFromItem - 1));
		_doesFollowPlayer = false;
		_doesFallToGround = true;
		_isAffectedByGravity = true;
		_doesFloatInPlace = false;
		_doesItemBounceOnGround = false;
		base.IsAffectedByTime = true;
		_maxFallSpeed = 400f;
		_gravityAcceleration = 1000f;
		_bboxOffset = new Point(-1, -2);
		Bbox = new Rectangle(Position.X, Position.Y, 18, 18);
		base.GlowBase = 1.25f;
	}

	public override void GetItem(Protagonist who)
	{
		bool flag = true;
		switch (_category)
		{
		case EInventoryCategoryType.Relic:
		{
			_level.UnlockRelic(_relicType);
			_level.AddScript(new ScriptAction(_relicType));
			flag = false;
			EInventoryRelicType relicType = _relicType;
			if (relicType == EInventoryRelicType.ScienceKeycardA)
			{
				AddDialogue("cs_gen_1_lun_20");
				AddDialogue("cs_gen_1_lun_21");
			}
			break;
		}
		case EInventoryCategoryType.UseItem:
			_level.GameSave.Inventory.UseItemInventory.AddItem(_itemData.Item, 1);
			_level.RequestItemGetPopup(_useItemType);
			NPCBase.TryShowQuestFinishedPopupByUseItem(_level, _useItemType);
			break;
		default:
			_level.RequestItemGetPopup(_equipmentType);
			_level.GameSave.Inventory.EquipmentInventory.AddItem(_itemData.Item, 1);
			break;
		}
		if (flag)
		{
			_level.PlayCue(ESFX.ItemGetGeneral, Bbox.Center);
		}
		IsFound = true;
		Kill();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_flashTimer += delta;
			if (_flashTimer > 1f)
			{
				base.IsGlowing = false;
				base.DrawColor = Color.White;
				if (_flashTimer > 3f)
				{
					_flashTimer = 0f;
				}
			}
			else
			{
				base.IsGlowing = true;
				float alpha = (float)(Math.Cos(_flashTimer * 18f) + 1.0) / 2f;
				base.GlowColor = new Color(1f, 1f, 1f, alpha);
			}
			if (IsInWater && _isAffectedByGravity)
			{
				_velocity.Y *= delta * 60f * 0.5f;
				_gravityAcceleration = 350f;
			}
		}
		base.Update(delta);
	}
}
