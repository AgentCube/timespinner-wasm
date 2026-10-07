using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Events.Cutscene;

namespace Timespinner.GameObjects.Events.Relics;

internal sealed class TimespinnerGearItem : RelicItemBase
{
	private const int StartOffsetY = 6;

	private const float RotationFrequency = 0.5f;

	private readonly EInventoryRelicType _gearType;

	public TimespinnerGearItem(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, SpriteSheet sprite, int gearIndex)
		: base(inLevel, new Point(inPosition.X, inPosition.Y + 6), inID, objectSpec, sprite)
	{
		DrawOrigin = new Vector2(14f, 14f);
		switch (gearIndex)
		{
		case 0:
			_gearType = EInventoryRelicType.TimespinnerGear1;
			break;
		case 1:
			_gearType = EInventoryRelicType.TimespinnerGear2;
			break;
		default:
			_gearType = EInventoryRelicType.TimespinnerGear3;
			break;
		}
		ChangeAnimation(3);
	}

	public override void Initialize()
	{
		if (_level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey((int)_gearType))
		{
			SilentKill();
		}
		else
		{
			base.Initialize();
		}
	}

	public override void Update(float delta)
	{
		float num = base.Rotation + delta * 0.5f;
		if (num >= (float)Math.PI * 2f)
		{
			num -= (float)Math.PI * 2f;
		}
		base.Rotation = num;
		base.Update(delta);
	}

	internal override void OnPickedUp()
	{
		int collectedTimespinnerGearCount = GetCollectedTimespinnerGearCount(_level.GameSave);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		AddLevelScriptAction(new ScriptAction(_gearType));
		_level.GameSave.UnlockRelic(_gearType);
		AddWaitScript(0.05f);
		switch (collectedTimespinnerGearCount)
		{
		case 0:
			DoFirstGearCutscene();
			break;
		case 1:
			DoSecondGearCutscene();
			break;
		case 2:
			DoThirdGearCutscene();
			break;
		}
	}

	internal static int GetCollectedTimespinnerGearCount(GameSave save)
	{
		int num = 0;
		Dictionary<int, InventoryRelic> inventory = save.Inventory.RelicInventory.Inventory;
		if (inventory.ContainsKey(3))
		{
			num++;
		}
		if (inventory.ContainsKey(4))
		{
			num++;
		}
		if (inventory.ContainsKey(5))
		{
			num++;
		}
		return num;
	}

	private void DoFirstGearCutscene()
	{
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Misc6_Gear1, _level, Position);
	}

	private void DoSecondGearCutscene()
	{
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Misc7_Gear2, _level, Position);
	}

	private void DoThirdGearCutscene()
	{
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Misc8_Gear3, _level, Position);
	}
}
