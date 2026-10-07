using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Gameplay;

public class ScriptAction
{
	internal bool HasStarted { get; set; }

	internal bool DoesBlockQueue { get; set; }

	internal bool IsUnskippable { get; set; }

	internal bool DoesClearSameType { get; set; }

	internal bool IsBeingSkipped { get; set; }

	internal EScriptType ScriptType { get; set; }

	internal EScriptActionType ActionType { get; set; }

	internal EScriptTargetType TargetType { get; set; }

	internal EInventoryCategoryType ItemToGiveType { get; set; }

	internal EOrbSlot OrbSlot { get; set; }

	internal int ItemToGive { get; set; }

	internal int ItemToGiveCount { get; set; }

	internal int IntArgument { get; set; }

	internal float SleepTime { get; set; }

	internal float ActionTimer { get; set; }

	internal float Duration { get; private set; }

	internal Vector4 Arguments { get; set; }

	internal Action Delegate { get; set; }

	internal Animate ScriptTarget { get; set; }

	internal DialogueBox Dialogue { get; private set; }

	internal AnimationSpec AnimationSpecification { get; private set; }

	internal CharacterSequenceSpecification CharacterSequence { get; private set; }

	internal LevelChangeRequest LevelRoomChangeRequest { get; private set; }

	internal bool IsActive => SleepTime <= 0f;

	internal bool IsFinished => ScriptType switch
	{
		EScriptType.Dialogue => Dialogue == null || Dialogue.IsFinished, 
		EScriptType.CutsceneStart => false, 
		_ => ActionTimer <= 0f, 
	};

	public ScriptAction()
	{
		DoesBlockQueue = true;
	}

	public ScriptAction(EScriptActionType actionType, float sleep, float duration, Vector4 args)
	{
		ActionType = actionType;
		SleepTime = sleep;
		ActionTimer = duration;
		Arguments = args;
		Duration = duration;
	}

	public ScriptAction(EScriptType scriptType, float duration, float sleepTime, bool doesBlock, Vector4 args)
	{
		ScriptType = scriptType;
		ActionTimer = duration;
		Duration = duration;
		DoesBlockQueue = doesBlock;
		SleepTime = sleepTime;
		Arguments = args;
	}

	public ScriptAction(DialogueBox newDialogue)
	{
		ScriptType = EScriptType.Dialogue;
		Dialogue = newDialogue;
		DoesBlockQueue = Dialogue.DoesBlockScriptEvents;
	}

	public ScriptAction(AnimationSpec newAnim, Animate who)
	{
		ScriptType = EScriptType.Animation;
		AnimationSpecification = newAnim;
		TargetType = EScriptTargetType.Specified;
		ScriptTarget = who;
		ActionTimer = newAnim.Speed * (float)newAnim.Length;
	}

	public ScriptAction(AnimationSpec newAnim, EScriptTargetType targetType)
	{
		ScriptType = EScriptType.Animation;
		AnimationSpecification = newAnim;
		TargetType = targetType;
		ActionTimer = newAnim.Speed * (float)newAnim.Length;
	}

	public ScriptAction(CharacterSequenceSpecification newSequence, Animate who)
	{
		ScriptType = EScriptType.CharacterSequence;
		CharacterSequence = newSequence;
		TargetType = EScriptTargetType.Specified;
		ScriptTarget = who;
		if (newSequence != null)
		{
			DoesBlockQueue = !newSequence.DoesRunConcurrently;
			if (DoesBlockQueue)
			{
				ActionTimer = newSequence.EstimateDuration();
			}
		}
	}

	public ScriptAction(Vector2 targetLocation, float duration, bool shouldBlock)
	{
		ScriptType = EScriptType.MoveCamera;
		Arguments = new Vector4(targetLocation.X, targetLocation.Y, 0f, 0f);
		ActionTimer = duration;
		Duration = duration;
		DoesBlockQueue = shouldBlock;
		DoesClearSameType = true;
	}

	internal ScriptAction(ESFX sfxToPlay, Point location)
	{
		ScriptType = EScriptType.PlayCue;
		Arguments = new Vector4((float)sfxToPlay, location.X, location.Y, 0f);
	}

	internal ScriptAction(EBGM songToPlay)
	{
		ScriptType = EScriptType.PlaySong;
		Arguments = new Vector4((float)songToPlay, 0f, 0f, 0f);
	}

	internal ScriptAction(EBGM songToPlay, bool shouldForceRestart, bool shouldStopPreviousSong)
	{
		ScriptType = EScriptType.PlaySong;
		Arguments = new Vector4((float)songToPlay, shouldForceRestart ? 1 : 0, shouldStopPreviousSong ? 1 : 0, 0f);
	}

	internal ScriptAction(EInventoryRelicType relicType)
	{
		ScriptType = EScriptType.RelicOrbGetToast;
		ItemToGiveType = EInventoryCategoryType.Relic;
		ItemToGive = (int)relicType;
		ItemToGiveCount = 1;
	}

	internal ScriptAction(EInventoryFamiliarType familiarType)
	{
		ScriptType = EScriptType.RelicOrbGetToast;
		ItemToGiveType = EInventoryCategoryType.Familiar;
		ItemToGive = (int)familiarType;
		ItemToGiveCount = 1;
	}

	internal ScriptAction(EInventoryOrbType orbType, EOrbSlot slot)
	{
		ScriptType = EScriptType.RelicOrbGetToast;
		ItemToGiveType = EInventoryCategoryType.Orb;
		ItemToGive = (int)orbType;
		ItemToGiveCount = 1;
		OrbSlot = slot;
	}

	internal ScriptAction(EInventoryEquipmentType equipmentType)
	{
		ScriptType = EScriptType.RelicOrbGetToast;
		ItemToGiveType = EInventoryCategoryType.Equipment;
		ItemToGive = (int)equipmentType;
		ItemToGiveCount = 1;
	}

	internal ScriptAction(NPCBase.ENPCType npcType, int questID)
	{
		ScriptType = EScriptType.QuestCompleteToast;
		ItemToGive = (int)npcType;
		ItemToGiveCount = questID;
		DoesBlockQueue = true;
	}

	internal ScriptAction(EInventoryUseItemType itemType, int howMany)
	{
		ScriptType = EScriptType.GiveItem;
		ItemToGiveType = EInventoryCategoryType.UseItem;
		ItemToGive = (int)itemType;
		ItemToGiveCount = howMany;
	}

	internal ScriptAction(EInventoryEquipmentType itemType, int howMany)
	{
		ScriptType = EScriptType.GiveItem;
		ItemToGiveType = EInventoryCategoryType.Equipment;
		ItemToGive = (int)itemType;
		ItemToGiveCount = howMany;
	}

	internal ScriptAction(EInventoryRelicType itemType, int howMany)
	{
		ScriptType = EScriptType.GiveItem;
		ItemToGiveType = EInventoryCategoryType.Relic;
		ItemToGive = (int)itemType;
		ItemToGiveCount = howMany;
	}

	internal ScriptAction(EInventoryOrbType itemType, int howMany)
	{
		ScriptType = EScriptType.GiveItem;
		ItemToGiveType = EInventoryCategoryType.Orb;
		ItemToGive = (int)itemType;
		ItemToGiveCount = howMany;
	}

	internal ScriptAction(EInventoryFamiliarType itemType, int howMany)
	{
		ScriptType = EScriptType.GiveItem;
		ItemToGiveType = EInventoryCategoryType.Familiar;
		ItemToGive = (int)itemType;
		ItemToGiveCount = howMany;
	}

	internal ScriptAction(EInventoryJournalType itemType, int howMany)
	{
		ScriptType = EScriptType.GiveItem;
		ItemToGiveType = EInventoryCategoryType.Journal;
		ItemToGive = (int)itemType;
		ItemToGiveCount = howMany;
	}

	public ScriptAction(LevelChangeRequest levelRoomChangeRequest)
	{
		ScriptType = EScriptType.LevelRoomChange;
		LevelRoomChangeRequest = levelRoomChangeRequest;
	}

	internal void Skip()
	{
		if (ScriptType != EScriptType.CutsceneStart)
		{
			ActionTimer = 0f;
			IsBeingSkipped = true;
			if (ScriptType == EScriptType.Dialogue && Dialogue != null)
			{
				Dialogue.Skip();
				Dialogue = null;
			}
		}
	}

	public virtual void Update(float delta)
	{
		if (SleepTime > 0f)
		{
			SleepTime -= delta;
		}
		if (SleepTime <= 0f)
		{
			ActionTimer -= delta;
			if (ActionTimer <= 0f)
			{
				ActionTimer = 0f;
			}
		}
	}
}
