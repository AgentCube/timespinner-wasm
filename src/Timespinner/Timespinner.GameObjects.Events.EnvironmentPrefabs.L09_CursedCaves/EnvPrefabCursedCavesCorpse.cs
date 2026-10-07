using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L09_CursedCaves;

internal sealed class EnvPrefabCursedCavesCorpse : EnvironmentPrefabBase
{
	private const EInventoryRelicType CardRelicType = EInventoryRelicType.ScienceKeycardB;

	private const int SparkleOffsetX = -2;

	private const int SparkleOffsetY = -3;

	private const float TimeBeforeAddingSparkle = 1f;

	private readonly BattleAnimation _sparkleAnimation;

	private readonly ItemPopupAppendage _itemPopupAppendage;

	private readonly PassiveBuffSparkleParticleSystem _sparklesParticles;

	private bool _hasKeycard;

	private float _sparkleTimer;

	public EnvPrefabCursedCavesCorpse(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscLab;
		ChangeAnimation(29);
		_bboxOffset = new Point(0, -8);
		Bbox = new Rectangle(0, 0, 32, 16);
		SnapBboxToPosition();
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
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
		_sparklesParticles = new PassiveBuffSparkleParticleSystem(_level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = new Vector4(0.9f, 0.5f, 0.25f, 1f)
		};
		_particleSystems.Add(_sparklesParticles);
		_sparkleAnimation = new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Position.X, Position.Y + -3), _level)
		{
			AnimationStart = 112,
			AnimationLength = 8
		};
	}

	public override void Initialize()
	{
		_hasKeycard = !_level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(14);
		base.Initialize();
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!base.IsFrozen && _hasKeycard && who is Protagonist protagonist)
		{
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
			if (protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				DoGetCutscene();
			}
		}
		return base.TriggerEvent(who, depth);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _hasKeycard)
		{
			_sparklesParticles.AddParticles(Position.ToVector2());
			_sparkleTimer += delta;
			if (_sparkleTimer >= 1f)
			{
				_sparkleTimer = 0f;
				_sparkleAnimation.Reset(new Point(Position.X + -2, Position.Y + -3), isFacingLeft: false);
				_level.AddAnimation(_sparkleAnimation);
			}
		}
		base.Update(delta);
	}

	private void DoGetCutscene()
	{
		_hasKeycard = false;
		AddDialogue("cs_corpse_lun_00");
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
			ActionType = EScriptActionType.Duck,
			ActionTimer = 3f,
			DoesBlockQueue = false
		});
		AddDialogue("cs_corpse_lun_01");
		AddDialogue("cs_corpse_lun_02");
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = UnlockKeycard
		});
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(EInventoryRelicType.ScienceKeycardB));
	}

	private void UnlockKeycard()
	{
		_level.UnlockRelic(EInventoryRelicType.ScienceKeycardB);
		int start = Math.Max((int)(InventoryItem.GetIconFromItem(EInventoryRelicType.ScienceKeycardB) - 1), 0);
		_itemPopupAppendage.ChangeAnimation(start);
		_itemPopupAppendage.IsPopppingUp = true;
		_appendages.Add(_itemPopupAppendage);
	}

	private void AddScript(ScriptAction newScript)
	{
		_level.AddScript(newScript);
	}
}
