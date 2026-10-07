using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class KeycardDoorEvent : GameEvent
{
	private const int MonitorGlowBase = 1;

	private const int BlackMonitorIndex = 12;

	private const int BlackLetterIndex = 20;

	private const int SparksEmissionOffsetX = 48;

	private const int SparksEmissionOffsetY = -80;

	private const float TimeToFlicker = 0.5f;

	private const float LetterGlowBase = 0.9f;

	private const float OrbGlowFrequency = (float)Math.PI;

	private const float TimeToWaitBeforeBeingClosed = 0.1f;

	private const float TimeToWaitBeforeErroringAgain = 1f;

	private const string SaveKey = "KEYCARD_{0}_{1}_{2}";

	private static readonly Point BlockingDimensions = new Point(16, 80);

	private static readonly Vector4 BaseScreenGlowColor = new Vector4(1f, 1f, 1f, 1f);

	private readonly bool _isBroken;

	private readonly EKeycardType _keycardType;

	private readonly Point _startKey;

	private readonly Appendage _monitorAppendage;

	private readonly Appendage _letterAppendage;

	private readonly Appendage _keypadAppendage;

	private readonly CharacterSequenceSpecification _openSequence;

	private readonly CharacterSequenceSpecification _alreadyOpenSequence;

	private readonly KeycardSparksParticleSystem _sparksParticles;

	private bool _isOpened;

	private bool _isFlickering;

	private float _flickerTimer;

	private float _errorTimer;

	private float _oscillDelta;

	private float _timeWithoutBeingTriggered;

	private Vector4 _screenGlowColor;

	internal string GetSaveKey => $"KEYCARD_{_level.ID}_{_level.RoomID}_{_startKey}";

	public KeycardDoorEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_startKey = inPosition;
		base.EventType = EEventTileType.KeycardDoor;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_sprite = _level.GCM.SpKeycardDoor;
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		Bbox = new Rectangle(0, 0, BlockingDimensions.X, BlockingDimensions.Y);
		_isAffectedByGravity = false;
		_isAffectedByTime = true;
		_isSolid = true;
		if (base.CharacterSpecification != null)
		{
			if (_appendages.Count > 7)
			{
				_monitorAppendage = _appendages[3];
				_letterAppendage = _appendages[4];
				_keypadAppendage = _appendages[5];
				_monitorAppendage.IsGlowing = true;
				_keypadAppendage.IsGlowing = true;
				_letterAppendage.IsGlowing = true;
				_monitorAppendage.GlowBase = 1f;
				_keypadAppendage.GlowBase = 1f;
				_letterAppendage.GlowBase = 0.9f;
				_letterAppendage.IsFacingLocked = true;
				if (!IsFacingLeft)
				{
					_letterAppendage.AnchorOffset = new Point(-_letterAppendage.AnchorOffset.X, _letterAppendage.AnchorOffset.Y);
				}
			}
			if (base.CharacterSpecification.Sequences.Count > 1)
			{
				_openSequence = base.CharacterSpecification.Sequences[0];
				_alreadyOpenSequence = base.CharacterSpecification.Sequences[1];
			}
		}
		_keycardType = (EKeycardType)objectSpec.Argument;
		switch (_keycardType)
		{
		case EKeycardType.A_Broken:
			_keycardType = EKeycardType.A_Black;
			_keypadAppendage.ChangeAnimation(16);
			_letterAppendage.ChangeAnimation(-1);
			_monitorAppendage.ChangeAnimation(12);
			SetCharacterSequenceByName("Break");
			_sparksParticles = new KeycardSparksParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_particleSystems.Add(_sparksParticles);
			_isBroken = true;
			break;
		case EKeycardType.A_Black:
			_monitorAppendage.ChangeAnimation(12);
			_keypadAppendage.ChangeAnimation(16);
			_letterAppendage.ChangeAnimation(20);
			break;
		case EKeycardType.B_Red:
			_monitorAppendage.ChangeAnimation(13);
			_keypadAppendage.ChangeAnimation(17);
			_letterAppendage.ChangeAnimation(21);
			break;
		case EKeycardType.C_Green:
			_monitorAppendage.ChangeAnimation(14);
			_keypadAppendage.ChangeAnimation(18);
			_letterAppendage.ChangeAnimation(22);
			break;
		case EKeycardType.D_Blue:
			_monitorAppendage.ChangeAnimation(15);
			_keypadAppendage.ChangeAnimation(19);
			_letterAppendage.ChangeAnimation(23);
			break;
		case EKeycardType.V_Pink:
			_monitorAppendage.ChangeAnimation(24);
			_keypadAppendage.ChangeAnimation(26);
			_letterAppendage.ChangeAnimation(25);
			break;
		}
	}

	public override void Initialize()
	{
		_isOpened = _level.GameSave.GetSaveBool(GetSaveKey);
		if (_isOpened)
		{
			_isSolid = false;
			if (_alreadyOpenSequence != null)
			{
				SetCharacterSequence(_alreadyOpenSequence);
			}
		}
		CreateCue(ESFX.DoorKeycardLoop, Position, isLooped: true)?.PlayWhenInRange();
		base.Initialize();
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag;
		if (_timeWithoutBeingTriggered < 0.1f && !_isOpened)
		{
			flag = CheckPlayerKeycard(_keycardType, _level);
			if (flag)
			{
				OpenAndSave(isImmediate: true);
			}
		}
		else
		{
			flag = base.TriggerEvent(who, depth);
		}
		return flag;
	}

	public override void Update(float delta)
	{
		if (_timeWithoutBeingTriggered < 0.1f)
		{
			_timeWithoutBeingTriggered += delta;
		}
		if (_errorTimer < 1f)
		{
			_errorTimer += delta;
		}
		if (!base.IsFrozen)
		{
			if (!_isOpened && _keypadAppendage != null)
			{
				Protagonist mainHero = _level.MainHero;
				if (mainHero != null && mainHero.Bbox.Intersects(_keypadAppendage.Bbox))
				{
					if (CheckPlayerKeycard(_keycardType, _level))
					{
						OpenAndSave(isImmediate: false);
					}
					else if (_errorTimer >= 1f)
					{
						PlayCue(ESFX.DoorKeycardError);
						if (_isBroken && !_isFlickering)
						{
							_isFlickering = true;
							_flickerTimer = 0f;
							_letterAppendage.ChangeAnimation(20);
							Vector2 where = new Vector2(Position.X + (IsFacingLeft ? 1 : (-1)) * 48, Position.Y + -80);
							_sparksParticles.AddParticles(where);
						}
						if (!CheckPlayerKeycard(EKeycardType.D_Blue, _level))
						{
							_level.ShowDialogueMessage("Keycard_Door_Error");
						}
					}
					_errorTimer = 0f;
				}
			}
			if (_monitorAppendage != null && _keypadAppendage != null)
			{
				_oscillDelta += delta;
				if (_oscillDelta > 10f)
				{
					_oscillDelta -= 10f;
				}
				_screenGlowColor = BaseScreenGlowColor;
				_screenGlowColor.W = (float)Math.Cos(_oscillDelta * (float)Math.PI) * 0.25f + 0.35f;
				Color glowColor = new Color(_screenGlowColor);
				_monitorAppendage.GlowColor = glowColor;
				_keypadAppendage.GlowColor = glowColor;
				_letterAppendage.GlowColor = glowColor;
			}
			if (_isFlickering)
			{
				_letterAppendage.IsGlowing = false;
				_letterAppendage.DoesInheritDrawColor = false;
				_flickerTimer += delta;
				if (_flickerTimer >= 0.5f)
				{
					_isFlickering = false;
					_letterAppendage.ChangeAnimation(-1);
				}
				else
				{
					float num = _flickerTimer / 0.5f;
					float num2 = (float)Math.Abs(Math.Sin(num * ((float)Math.PI * 2f)));
					_letterAppendage.DrawColor = Color.White * num2;
				}
			}
		}
		base.Update(delta);
	}

	private void OpenAndSave(bool isImmediate)
	{
		if (!isImmediate && _openSequence != null)
		{
			SetCharacterSequence(_openSequence);
			PlayCue(ESFX.DoorKeycardAccessGranted);
			PlayCue(ESFX.DoorKeycardOpen);
		}
		else if (isImmediate && _alreadyOpenSequence != null)
		{
			SetCharacterSequence(_alreadyOpenSequence);
		}
		_isOpened = true;
		_isSolid = false;
		_level.GameSave.SetValue(GetSaveKey, value: true);
	}

	internal void RemotelyOpenDoor()
	{
		OpenAndSave(isImmediate: false);
	}

	private static bool CheckPlayerKeycard(EKeycardType keycardType, Level level)
	{
		bool result = false;
		switch (keycardType)
		{
		case EKeycardType.A_Black:
			result = level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardA);
			break;
		case EKeycardType.B_Red:
			result = level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardB) || level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardA);
			break;
		case EKeycardType.C_Green:
			result = level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardC) || level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardB) || level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardA);
			break;
		case EKeycardType.D_Blue:
			result = level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardD) || level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardC) || level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardB) || level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardA);
			break;
		case EKeycardType.V_Pink:
			result = level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ScienceKeycardV);
			break;
		}
		return result;
	}

	internal static void SaveDoorIsOpen(int levelID, int room, Point position, Level level)
	{
		level.GameSave.SetValue($"KEYCARD_{levelID}_{room}_{position}", value: true);
	}
}
