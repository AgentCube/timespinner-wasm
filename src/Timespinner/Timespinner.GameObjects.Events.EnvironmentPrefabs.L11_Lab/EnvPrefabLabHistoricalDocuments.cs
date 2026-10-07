using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabLabHistoricalDocuments : EnvironmentPrefabBase
{
	private const int DrawerStartIndex = 42;

	private const int DrawerAnimationLength = 4;

	private const int PlayerStandingOffset = 24;

	private const string SaveKey = "AreLabDocumentsFound";

	private const EInventoryUseItemType DocumentsItemType = EInventoryUseItemType.HistoricalDocuments;

	private readonly ItemPopupAppendage _itemPopupAppendage;

	private readonly PassiveBuffSparkleParticleSystem _sparklesParticles;

	private bool _hasDocuments;

	public EnvPrefabLabHistoricalDocuments(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscLab;
		ChangeAnimation(42);
		Position = new Point(Position.X - 3, Position.Y - 1);
		Bbox = new Rectangle(0, 0, 27, 7);
		SnapBboxToPosition();
		IsFacingLeft = false;
		_isSolid = false;
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = true;
		_doAppendagesMatchImageFacing = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_itemPopupAppendage = new ItemPopupAppendage(this, _level, _level.GCM.SpMenuIcons)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			ShouldPlaySFX = false
		};
		_sparklesParticles = new PassiveBuffSparkleParticleSystem(_level.GCM.TxParticleEnergy, 5)
		{
			BaseColor = new Vector4(0.9f, 0.9f, 0.9f, 1f)
		};
		_particleSystems.Add(_sparklesParticles);
	}

	public override void Initialize()
	{
		_hasDocuments = _level.GameSave.GetSaveBool("AreLabDocumentsFound");
		if (!_hasDocuments)
		{
			_isGlowing = true;
			base.GlowColor = new Color(1f, 1f, 1f, 0.8f);
		}
		base.Initialize();
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!base.IsFrozen && !_hasDocuments && who is Protagonist protagonist)
		{
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
			if (protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				int primaryQuestState = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Medic, _level.GameSave);
				int subQuestState = NPCBase.GetSubQuestState(NPCBase.ENPCType.Medic, _level.GameSave);
				AnimationSpec animationSpec = new AnimationSpec();
				animationSpec.Start = 42;
				animationSpec.Length = 4;
				animationSpec.Speed = 0.1f;
				animationSpec.Type = EAnimationType.Once;
				AnimationSpec newAnim = animationSpec;
				MovePlayerToTalkingPosition();
				AddScript(new ScriptAction(newAnim, this));
				if (primaryQuestState == 4 && subQuestState > 0)
				{
					DoGetCutscene();
				}
				else
				{
					DoIgnoreCutscene();
				}
				_isGlowing = false;
				base.DrawColor = Color.White;
				AnimationSpec animationSpec2 = new AnimationSpec();
				animationSpec2.Start = 42;
				animationSpec2.Length = 4;
				animationSpec2.Speed = 0.1f;
				animationSpec2.Type = EAnimationType.Once;
				animationSpec2.IsInReverse = true;
				AnimationSpec newAnim2 = animationSpec2;
				AddScript(new ScriptAction(newAnim2, this));
			}
		}
		return base.TriggerEvent(who, depth);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_hasDocuments)
		{
			_sparklesParticles.AddParticles(Position.ToVector2());
		}
		base.Update(delta);
	}

	private void DoIgnoreCutscene()
	{
		_hasDocuments = true;
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		AddDialogue("q_ram_4_lun_29alt");
	}

	private void DoGetCutscene()
	{
		_hasDocuments = true;
		_level.GameSave.SetValue("AreLabDocumentsFound", value: true);
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		AddDialogue("q_ram_4_lun_29");
		AddDialogue("q_ram_4_lun_30");
		AddDialogue("q_ram_4_lun_31");
		_level.AddScript(new ScriptAction(ESFX.ItemGetGeneral, Position));
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = GetDocuments
		});
		AddWaitScript(0.5f);
		_level.AddScript(new ScriptAction(EInventoryUseItemType.HistoricalDocuments, 1));
	}

	private void GetDocuments()
	{
		int start = Math.Max((int)(InventoryItem.GetIconFromItem(EInventoryUseItemType.HistoricalDocuments) - 1), 0);
		_itemPopupAppendage.ChangeAnimation(start);
		_itemPopupAppendage.IsPopppingUp = true;
		_appendages.Add(_itemPopupAppendage);
	}

	private void AddScript(ScriptAction newScript)
	{
		_level.AddScript(newScript);
	}

	private void MovePlayerToTalkingPosition()
	{
		Point playerPosition = _level.GetPlayerPosition();
		Point point = new Point(Position.X + 24, Position.Y);
		bool flag = playerPosition.X < point.X;
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 1f,
			DoesBlockQueue = true,
			Arguments = new Vector4(point.X, point.Y, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.1f,
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		if (flag)
		{
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.LookDirection,
				ActionTimer = 0.25f,
				DoesBlockQueue = true,
				Arguments = new Vector4(-1f, 0f, 0f, 0f)
			});
		}
	}
}
