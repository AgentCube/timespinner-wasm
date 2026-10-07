using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;

namespace Timespinner.GameObjects.Events.Relics;

internal class PyramidKeys : RelicItemBase
{
	private const EInventoryRelicType RelicType = EInventoryRelicType.PyramidsKey;

	private const int RelicTypeKey = 6;

	private const int KeySpinRadius = 9;

	private const float KeySpinFrequency = 1f;

	private readonly Point _rotationCenter;

	private readonly Appendage _leftKeyAppendage;

	private readonly Appendage _rightKeyAppendage;

	private float _keyTimer;

	public PyramidKeys(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, SpriteSheet sprite)
		: base(inLevel, inPosition, inID, objectSpec, sprite)
	{
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		_doAppendagesInheritDrawColor = true;
		base.DoesDrawAura = false;
		_rotationCenter = inPosition;
		_leftKeyAppendage = new Appendage(this, new Point(16, 16), Point.Zero, _level, _sprite)
		{
			IsFacingLeft = true,
			DrawOrigin = new Vector2(8f, 8f),
			DoesDrawTrail = true,
			TrailLength = 6,
			TrailFadeRate = 2f
		};
		_rightKeyAppendage = new Appendage(this, new Point(16, 16), Point.Zero, _level, _sprite)
		{
			IsFacingLeft = false,
			DrawOrigin = new Vector2(8f, 8f),
			DoesDrawTrail = true,
			TrailLength = 6,
			TrailFadeRate = 2f
		};
		_leftKeyAppendage.ChangeAnimation(21, 5, 0.1f, EAnimationType.Cycle);
		_rightKeyAppendage.ChangeAnimation(21, 5, 0.1f, EAnimationType.Cycle);
		base.Appendages.Add(_leftKeyAppendage);
		base.Appendages.Add(_rightKeyAppendage);
	}

	public override void Initialize()
	{
		if (_level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(6))
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
		_keyTimer += delta * 1f;
		if (_keyTimer >= (float)Math.PI * 2f)
		{
			_keyTimer -= (float)Math.PI * 2f;
		}
		float num = (float)Math.Cos(_keyTimer) * 9f;
		float num2 = (float)Math.Sin(_keyTimer);
		float num3 = num2 * 9f;
		_leftKeyAppendage.Rotation = num2;
		_rightKeyAppendage.Rotation = num2;
		_leftKeyAppendage.Position = new Point(_rotationCenter.X + (int)num, _rotationCenter.Y + (int)num3);
		_rightKeyAppendage.Position = new Point(_rotationCenter.X - (int)num, _rotationCenter.Y - (int)num3);
		if (base.IsFading)
		{
			Color drawColor = Color.White * (1f - base.FadePercentage);
			_doAppendagesInheritDrawColor = false;
			_leftKeyAppendage.DrawColor = drawColor;
			_rightKeyAppendage.DrawColor = drawColor;
		}
		if (base.IsFrozen)
		{
			_isFrozen = false;
		}
		base.Update(delta);
	}

	internal override void OnPickedUp()
	{
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		AddLevelScriptAction(new ScriptAction(EInventoryRelicType.PyramidsKey));
		_level.GameSave.UnlockRelic(EInventoryRelicType.PyramidsKey);
		AddWaitScript(0.05f);
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.LakeSerene2_Warp, _level, Position);
	}
}
