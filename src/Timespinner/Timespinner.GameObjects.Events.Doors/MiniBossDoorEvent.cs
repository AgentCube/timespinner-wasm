using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class MiniBossDoorEvent : SlidingDoorEvent
{
	private const float OscillFrequencyY = (float)Math.PI;

	private const float OrbGlowFrequency = (float)Math.PI;

	private const float TimeToWaitBeforeBeingClosed = 0.1f;

	private static readonly Vector4 BaseOrbGlowColor = new Vector4(1f, 0.9f, 0.95f, 1f);

	private readonly Appendage _stoneAppendage;

	private float _oscillDelta;

	private float _timeWithoutBeingTriggered;

	public MiniBossDoorEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_isLocked = false;
		_sprite = _level.GCM.SpSlidingDoors;
		base.EventType = EEventTileType.MiniBossDoor;
		IsFacingLeft = true;
		base.CanBeTriggeredByFamiliar = true;
		ChangeAnimation(14);
		_stoneAppendage = new Appendage(this, new Point(16, 80), Point.Zero, _level, _sprite)
		{
			AnchorObject = this,
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_stoneAppendage.ChangeAnimation(13);
		base.Appendages.Add(_stoneAppendage);
	}

	public override void Initialize()
	{
		bool flag = false;
		GameSave gameSave = _level.GameSave;
		if (_level.ID == 7)
		{
			bool saveBool = gameSave.GetSaveBool("IsCantoranActive");
			bool saveBool2 = gameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Cantoran));
			bool saveBool3 = gameSave.GetSaveBool(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Quartermaster));
			flag = (!saveBool || saveBool2) && saveBool3;
		}
		else if (_level.ID == 11)
		{
			bool saveBool4 = gameSave.GetSaveBool("11_LabPower");
			bool saveBool5 = gameSave.GetSaveBool("11_Exp13");
			flag = !saveBool4 || saveBool5;
		}
		if (flag)
		{
			_doorState = ESlidingDoorState.Opened;
			base.IsOpenForever = true;
			SetPositionToOpen();
		}
		base.Initialize();
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = false;
		if (_timeWithoutBeingTriggered < 0.1f)
		{
			OpenDoor(-1f);
		}
		else
		{
			result = base.TriggerEvent(who, depth);
		}
		return result;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_timeWithoutBeingTriggered < 0.1f)
			{
				_timeWithoutBeingTriggered += delta;
			}
			if (!base.IsLocked)
			{
				_oscillDelta += delta * (float)Math.PI;
				if (_oscillDelta >= (float)Math.PI * 2f)
				{
					_oscillDelta -= (float)Math.PI * 2f;
				}
				Vector4 baseOrbGlowColor = BaseOrbGlowColor;
				baseOrbGlowColor.W = (float)Math.Cos(_oscillDelta) * 0.2f + 0.8f;
				_stoneAppendage.IsGlowing = true;
				_stoneAppendage.GlowColor = new Color(baseOrbGlowColor);
			}
			else
			{
				_stoneAppendage.IsGlowing = false;
			}
		}
		base.Update(delta);
	}
}
