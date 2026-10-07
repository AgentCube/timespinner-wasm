using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Events.Doors;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events;

public sealed class TheTimespinner : GameEvent
{
	internal enum ETimespinnerState
	{
		Off,
		StartingSlow,
		StartingFast,
		Haywire,
		Dead
	}

	private const int DetachCount = 4;

	private const float TimeBetweenPieceDetach = 0.1f;

	private const float TimeToDetach = 0.4f;

	private const float TimeToWait = 0.2f;

	private const float TimeToAbsorb = 1f;

	private const float TimeBeforeAbsorbing = 0.6f;

	private const float TimeForEntireUnhingeSequence = 1.6f;

	private const float TimeBeforeCoolingDown = 0.5f;

	private const float WheelRotationSpeedStartingSlow = 8f;

	private const float WheelRotationSpeedStartingFast = 15f;

	private const float WheelRotationSpeedHaywire = 30f;

	private const float WheelRotationSpeedDead = 30f;

	private const float EnergyPulseRateStartingSlow = 2f;

	private const float EnergyPulseRateStartingFast = 4f;

	private const float EnergyPulseRateHaywire = 8f;

	private const float EnergyPulseRateDead = 0f;

	private const float TimeBetweenEnergyStates = 2f;

	private static readonly Point PortalOffset = new Point(8, -90);

	private static readonly Vector4 GradientColorOff1 = new Vector4(0.075f, 0.05f, 0.075f, 1f);

	private static readonly Vector4 GradientColorOff2 = new Vector4(0.125f, 0.1f, 0.125f, 1f);

	private static readonly Vector4 GradientColorStartingSlow1 = new Vector4(0.4f, 0.35f, 0.45f, 1f);

	private static readonly Vector4 GradientColorStartingSlow2 = new Vector4(0.25f, 0.2f, 0.6f, 1f);

	private static readonly Vector4 GradientColorStartingFast1 = new Vector4(0.75f, 0.7f, 0.9f, 1f);

	private static readonly Vector4 GradientColorStartingFast2 = new Vector4(0.3f, 0.2f, 0.8f, 1f);

	private static readonly Vector4 GradientColorHaywire1 = new Vector4(0.9f, 0.8f, 0.85f, 1f);

	private static readonly Vector4 GradientColorHaywire2 = new Vector4(0.8f, 0.2f, 0.35f, 1f);

	private static readonly Vector4 GradientColorDead1 = new Vector4(0.45f, 0.4f, 0.4f, 1f);

	private static readonly Vector4 GradientColorDead2 = new Vector4(0.4f, 0.1f, 0.05f, 1f);

	private static readonly Vector4 FloorSparkleParticlesColor = new Vector4(0.5f, 0.5f, 0.75f, 1f);

	private static readonly Color FloorLightDrawColor = new Color(0.1f, 0.125f, 0.2f, 0.15f);

	private readonly bool _isBroken;

	private readonly bool _hasPlayerBeenToThePast;

	private readonly GlowingFloorEvent _cityGlowingFloorEvent;

	private readonly TimeGateEvent _timeGate;

	private readonly TimespinnerAppendage _wheelAppendage;

	private readonly TimespinnerAppendage _spindleAppendage;

	private readonly List<TimespinnerGear> _gearAppendages = new List<TimespinnerGear>();

	private bool _isUnhinging;

	private ETimespinnerState _timespinnerState;

	private ETimespinnerState _lastTimespinnerState;

	private float _wheelRotationSpeed;

	private float _energyPulseRate;

	private float _energyPulsePercent;

	private float _stateTransitionTimer;

	private float _cooldownTimer;

	private float _unhingeTimer;

	private Vector4 _gradientColor1;

	private Vector4 _gradientColor2;

	private SFXCueInstance _normalLoopCueInstance;

	private SFXCueInstance _haywireLoopCueInstance;

	private GyrePortalEvent _gyrePortal;

	public Point PortalCenter => _timeGate.WarpInPoint;

	public TheTimespinner(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition.Add(8, 8), inID, objectSpec)
	{
		base.EventType = EEventTileType.TheTimespinner;
		_isBroken = objectSpec != null && objectSpec.Argument == 1;
		if (_isBroken)
		{
			Position = Position.Add(0, 6);
		}
		_sprite = (_isBroken ? _level.GCM.SpCityTimespinner : _level.GCM.SpTheTimespinner);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 200, 206);
		base.DoesCollideWithTiles = false;
		_doesDrawBaseSprite = true;
		_doesDrawAppendages = false;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		IsFacingLeft = true;
		_isAffectedByGravity = false;
		base.IsAffectedByTime = _isBroken;
		_doesPersist = true;
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		_timeGate = new TimeGateEvent(_level, Position.Add(PortalOffset), _level.NextObjectTicketID, new ObjectTileSpecification(481))
		{
			GateScriptType = TimeGateEvent.EGateScriptType.Closed,
			TargetLevelID = 8
		};
		_wheelAppendage = new TimespinnerAppendage(this, new Point(1, 1), new Point(25, 25), _level, _sprite, TimespinnerAppendage.ETimespinnerAppendageType.Wheel)
		{
			DrawOrigin = new Vector2(25.5f, 25.5f)
		};
		_spindleAppendage = new TimespinnerAppendage(this, new Point(1, 1), new Point(11, 11), _level, _sprite, TimespinnerAppendage.ETimespinnerAppendageType.Spindle)
		{
			DrawOrigin = new Vector2(10.5f, 10.5f)
		};
		_hasPlayerBeenToThePast = _level.GameSave.GetSaveBool("HasUsedCityTS");
		if (_isBroken)
		{
			_wheelAppendage.ChangeAnimation(-1);
			_spindleAppendage.ChangeAnimation(-1);
			if (_hasPlayerBeenToThePast)
			{
				ChangeAnimation(-1);
			}
			else if (_level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(2))
			{
				_cityGlowingFloorEvent = new GlowingFloorEvent(_level, Position, FloorLightDrawColor, FloorSparkleParticlesColor);
				_level.RequestAddObject(_cityGlowingFloorEvent);
			}
		}
		for (int i = 0; i < 3; i++)
		{
			TimespinnerGear timespinnerGear = new TimespinnerGear(this, _level, _sprite, i);
			_gearAppendages.Add(timespinnerGear);
			if (_isBroken)
			{
				timespinnerGear.ChangeAnimation(-1);
			}
		}
		InitializeAppendages();
	}

	private void InitializeAppendages()
	{
		_appendages.Add(_wheelAppendage);
		_appendages.Add(_spindleAppendage);
		foreach (TimespinnerGear gearAppendage in _gearAppendages)
		{
			_appendages.Add(gearAppendage);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _cooldownTimer > 0f)
		{
			_cooldownTimer -= delta;
		}
		UpdateWheelRotations(delta);
		UpdateTimespinnerState(delta);
		UpdateUnhinging(delta);
		base.Update(delta);
		_timeGate.Update(delta);
		if (_gyrePortal != null)
		{
			_gyrePortal.Update(delta);
		}
	}

	private void UpdateWheelRotations(float delta)
	{
		float rotation = _wheelRotationSpeed * delta;
		_wheelAppendage.IncrementRotation(rotation);
		_spindleAppendage.IncrementRotation(rotation);
		foreach (TimespinnerGear gearAppendage in _gearAppendages)
		{
			gearAppendage.IncrementRotation(rotation);
		}
	}

	private void UpdateTimespinnerState(float delta)
	{
		float wheelRotationSpeedByState = GetWheelRotationSpeedByState(_timespinnerState);
		float rateByState = GetRateByState(_timespinnerState);
		Vector4 colorByState = GetColorByState(_timespinnerState, isFirstColor: true);
		Vector4 colorByState2 = GetColorByState(_timespinnerState, isFirstColor: false);
		if (_stateTransitionTimer > 0f)
		{
			_stateTransitionTimer -= delta;
			if (_stateTransitionTimer < 0f)
			{
				_stateTransitionTimer = 0f;
			}
			float wheelRotationSpeedByState2 = GetWheelRotationSpeedByState(_lastTimespinnerState);
			float rateByState2 = GetRateByState(_lastTimespinnerState);
			Vector4 colorByState3 = GetColorByState(_lastTimespinnerState, isFirstColor: true);
			Vector4 colorByState4 = GetColorByState(_lastTimespinnerState, isFirstColor: false);
			float amount = _stateTransitionTimer / 2f;
			_wheelRotationSpeed = MathHelper.Lerp(wheelRotationSpeedByState, wheelRotationSpeedByState2, amount);
			_energyPulseRate = MathHelper.Lerp(rateByState, rateByState2, amount);
			_gradientColor1 = Vector4.Lerp(colorByState, colorByState3, amount);
			_gradientColor2 = Vector4.Lerp(colorByState2, colorByState4, amount);
		}
		else
		{
			_wheelRotationSpeed = wheelRotationSpeedByState;
			_energyPulseRate = rateByState;
			_gradientColor1 = colorByState;
			_gradientColor2 = colorByState2;
		}
		_energyPulsePercent += delta * _energyPulseRate;
		if (_energyPulsePercent > 1f)
		{
			_energyPulsePercent = _energyPulsePercent.Mod(1f);
		}
	}

	private void UpdateUnhinging(float delta)
	{
		if (!_isUnhinging)
		{
			return;
		}
		float unhingeTimer = _unhingeTimer;
		_unhingeTimer += delta;
		if (_unhingeTimer <= 0.4f)
		{
			if (_unhingeTimer >= 0.1f && unhingeTimer < 0.1f)
			{
				_spindleAppendage.Unhinge();
			}
			if (_unhingeTimer >= 0.2f && unhingeTimer < 0.2f)
			{
				_wheelAppendage.Unhinge();
			}
			if (!(_unhingeTimer >= 0.3f) || !(unhingeTimer < 0.3f))
			{
				return;
			}
			{
				foreach (TimespinnerGear gearAppendage in _gearAppendages)
				{
					gearAppendage.Unhinge();
				}
				return;
			}
		}
		if (_unhingeTimer <= 0.6f || !(_unhingeTimer <= 1.6f) || !(unhingeTimer <= 0.6f))
		{
			return;
		}
		_spindleAppendage.Absorb();
		_wheelAppendage.Absorb();
		foreach (TimespinnerGear gearAppendage2 in _gearAppendages)
		{
			gearAppendage2.Absorb();
		}
	}

	internal void ChangeTimespinnerState(ETimespinnerState newState)
	{
		_lastTimespinnerState = _timespinnerState;
		_timespinnerState = newState;
		_stateTransitionTimer = 2f;
		switch (newState)
		{
		case ETimespinnerState.StartingSlow:
		case ETimespinnerState.StartingFast:
			if (_normalLoopCueInstance == null)
			{
				_normalLoopCueInstance = CreateCue(ESFX.CsTimespinnerNormalLoop, Position, isLooped: true);
				if (_normalLoopCueInstance != null)
				{
					_normalLoopCueInstance.FadeIn(0.5f);
					_normalLoopCueInstance.PlayWhenInRange();
				}
			}
			break;
		case ETimespinnerState.Haywire:
			if (_normalLoopCueInstance != null)
			{
				_normalLoopCueInstance.Stop(0.25f);
			}
			if (_haywireLoopCueInstance == null)
			{
				_haywireLoopCueInstance = CreateCue(ESFX.CsTimespinnerHaywireLoop, Position, isLooped: true);
				if (_haywireLoopCueInstance != null)
				{
					_haywireLoopCueInstance.FadeIn(0.25f);
					_haywireLoopCueInstance.PlayWhenInRange();
				}
			}
			break;
		}
		UpdateTimespinnerState(0f);
	}

	internal void UnhingeAllPieces()
	{
		_isUnhinging = true;
		_unhingeTimer = 0f;
	}

	internal void OpenTimeGate()
	{
		if (_timeGate != null)
		{
			_timeGate.OpenAndStayOpen();
		}
	}

	internal void CloseTimeGate()
	{
		if (_timeGate != null)
		{
			_timeGate.CloseGate();
		}
	}

	internal void SetTimeGateTargetLevel(int targetLevel)
	{
		if (_timeGate != null)
		{
			_timeGate.ChangeTargetLevel(targetLevel);
		}
	}

	private static float GetWheelRotationSpeedByState(ETimespinnerState state)
	{
		float result = 0f;
		switch (state)
		{
		case ETimespinnerState.StartingSlow:
			result = 8f;
			break;
		case ETimespinnerState.StartingFast:
			result = 15f;
			break;
		case ETimespinnerState.Haywire:
			result = 30f;
			break;
		case ETimespinnerState.Dead:
			result = 30f;
			break;
		}
		return result;
	}

	private static float GetRateByState(ETimespinnerState state)
	{
		float result = 0f;
		switch (state)
		{
		case ETimespinnerState.StartingSlow:
			result = 2f;
			break;
		case ETimespinnerState.StartingFast:
			result = 4f;
			break;
		case ETimespinnerState.Haywire:
			result = 8f;
			break;
		case ETimespinnerState.Dead:
			result = 0f;
			break;
		}
		return result;
	}

	private static Vector4 GetColorByState(ETimespinnerState state, bool isFirstColor)
	{
		Vector4 result = Vector4.Zero;
		switch (state)
		{
		case ETimespinnerState.StartingSlow:
			result = (isFirstColor ? GradientColorStartingSlow1 : GradientColorStartingSlow2);
			break;
		case ETimespinnerState.StartingFast:
			result = (isFirstColor ? GradientColorStartingFast1 : GradientColorStartingFast2);
			break;
		case ETimespinnerState.Haywire:
			result = (isFirstColor ? GradientColorHaywire1 : GradientColorHaywire2);
			break;
		case ETimespinnerState.Off:
			result = (isFirstColor ? GradientColorOff1 : GradientColorOff2);
			break;
		case ETimespinnerState.Dead:
			result = (isFirstColor ? GradientColorDead1 : GradientColorDead2);
			break;
		}
		return result;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_level.GCM.EfEnergyPulse.Parameters["colorOne"].SetValue(_gradientColor1);
		_level.GCM.EfEnergyPulse.Parameters["colorTwo"].SetValue(_gradientColor2);
		_level.GCM.EfEnergyPulse.Parameters["basePercentage"].SetValue(_energyPulsePercent);
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfEnergyPulse);
		base.Draw(spriteBatch);
		foreach (Appendage appendage in base.Appendages)
		{
			if (_doAppendagesInheritDrawColor && appendage.DoesInheritDrawColor && !appendage.IsDebugBlinking)
			{
				appendage.DrawColor = base.DrawColor;
			}
			appendage.Draw(spriteBatch);
		}
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		_timeGate.Draw(spriteBatch);
		if (_gyrePortal != null)
		{
			_gyrePortal.Draw(spriteBatch);
		}
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = base.TriggerEvent(who, depth);
		if (_isBroken && !_hasPlayerBeenToThePast)
		{
			if (who is Protagonist protagonist && _cooldownTimer <= 0f)
			{
				_level.RequestButtonPrompt(4, new Point(Position.X, Position.Y - 32));
				if (protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
				{
					StartTeleportToForest();
				}
			}
		}
		else
		{
			base.CanBeTriggered = false;
		}
		return result;
	}

	internal void AddWheelAndSpindle()
	{
		_wheelAppendage.ChangeAnimation(1);
		_spindleAppendage.ChangeAnimation(2);
		_wheelAppendage.ChangeSize(isShrinking: false);
		_spindleAppendage.ChangeSize(isShrinking: false);
		ChangeTimespinnerState(ETimespinnerState.StartingFast);
	}

	internal void RemoveWheelAndSpindle()
	{
		_wheelAppendage.ChangeSize(isShrinking: true);
		_spindleAppendage.ChangeSize(isShrinking: true);
		ChangeTimespinnerState(ETimespinnerState.Dead);
	}

	internal SoulStreamEvent AddSoulStream(bool isAnchoredToPlayer)
	{
		return _wheelAppendage.AddSoulStream(isAnchoredToPlayer);
	}

	internal SoulStreamEvent AddSoulStream(Mobile anchorObject)
	{
		SoulStreamEvent soulStreamEvent = AddSoulStream(isAnchoredToPlayer: false);
		if (soulStreamEvent != null)
		{
			soulStreamEvent.AlternateStreamAnchor = anchorObject;
		}
		return soulStreamEvent;
	}

	private void StartTeleportToForest()
	{
		_cooldownTimer = 0.5f;
		Dictionary<int, InventoryRelic> inventory = _level.GameSave.Inventory.RelicInventory.Inventory;
		bool flag = inventory.ContainsKey(2);
		bool flag2 = inventory.ContainsKey(1);
		if (flag2 && flag)
		{
			if (_cityGlowingFloorEvent != null)
			{
				_cityGlowingFloorEvent.FadeOut();
			}
			_cooldownTimer = 1000f;
			_level.GameSave.SetValue("HasUsedCityTS", value: true);
			CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.City3_Warp, _level, Position);
		}
		else
		{
			AddDialogue(flag2 ? "cs_cit_1_lun_06" : "cs_cit_1_lun_06b");
		}
	}

	internal void SetGyrePortal(GyrePortalEvent gyrePortal)
	{
		_gyrePortal = gyrePortal;
	}

	internal void HidePieces()
	{
		_wheelAppendage.ChangeSize(isShrinking: true);
		_spindleAppendage.ChangeSize(isShrinking: true);
		foreach (TimespinnerGear gearAppendage in _gearAppendages)
		{
			gearAppendage.ChangeSize(isShrinking: true);
		}
	}
}
