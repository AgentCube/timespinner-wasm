using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class TransitionDoorEvent : SlidingDoorEvent
{
	private const int DoorSegmentHeight = 40;

	private const int DoorOpenOffsetHeight = 48;

	private const float TimeToOpen = 0.5f;

	private const float TimeToClose = 0.39f;

	private const float TimeToWaitBeforeBeingClosed = 0.1f;

	private const float GlowFrequency = 2f;

	private static readonly Color BlueGemGlowColor = new Color(200, 230, 255);

	private static readonly Color PinkGemGlowColor = new Color(255, 200, 219);

	private readonly bool _isRoyalDoor;

	private readonly Point _startingPosition;

	private readonly Color _baseGemGlowColor;

	private readonly Appendage _topHalf;

	private readonly Appendage _mainGem;

	private readonly List<Appendage> _gems = new List<Appendage>();

	private float _glowTimer;

	private float _openCloseTimer;

	private float _timeWithoutBeingTriggered;

	private bool _hasExitedDoor;

	private BattleAnimation _rippleAnimation;

	public TransitionDoorEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification specification)
		: base(inLevel, inPosition, inID, specification)
	{
		_isRoyalDoor = specification.Argument > 0;
		_baseGemGlowColor = (_isRoyalDoor ? PinkGemGlowColor : BlueGemGlowColor);
		_isLocked = false;
		_sprite = _level.GCM.SpSlidingDoors;
		IsFacingLeft = !specification.IsFlippedHorizontally;
		Bbox = new Rectangle(Position.X, Position.Y, 16, 40);
		base.TriggerBbox = new Rectangle(Position.X, Position.Y, 16, 80);
		_doAppendagesMatchImageFacing = false;
		_doAppendagesInheritDrawColor = false;
		IsFlippedVertically = true;
		_startingPosition = inPosition;
		_isAffectedByTime = false;
		base.CannotBeGrabbed = true;
		ChangeAnimation(_isRoyalDoor ? 9 : 2);
		if (base.CharacterSpecification == null || base.Appendages.Count <= 1)
		{
			return;
		}
		_topHalf = base.Appendages[0];
		_topHalf.Position = new Point(Position.X, Position.Y - 40);
		_mainGem = base.Appendages[2];
		_mainGem.IsGlowing = true;
		_mainGem.Position = Position.Add(-1, -32);
		if (_isRoyalDoor)
		{
			_topHalf.ChangeAnimation(9);
			_mainGem.ChangeAnimation(10);
		}
		foreach (Appendage appendage in _topHalf.Appendages)
		{
			_gems.Add(appendage);
		}
		_gems.Add(base.Appendages[1]);
		foreach (Appendage gem in _gems)
		{
			gem.IsGlowing = true;
			gem.GlowBase = 1.5f;
			gem.GlowColor = Color.White;
			if (_isRoyalDoor)
			{
				gem.ChangeAnimation(11);
			}
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = false;
		if (!_hasExitedDoor && _timeWithoutBeingTriggered < 2.0f)
		{
			_hasExitedDoor = true;
			DoExitScript();
		}
		else
		{
			if (_isRoyalDoor)
			{
				_isLocked = !IsPlayerHoldingRoyalOrb();
			}
			result = base.TriggerEvent(who, depth);
			if (!_isRoyalDoor)
			{
				_level.ClearLevelSaveData();
			}
		}
		return result;
	}

	private bool IsPlayerHoldingRoyalOrb()
	{
		PlayerInventory inventory = _level.GameSave.Inventory;
		if (inventory.EquippedMeleeOrbA != EInventoryOrbType.Pink && inventory.EquippedMeleeOrbB != EInventoryOrbType.Pink && inventory.EquippedSpellOrb != EInventoryOrbType.Pink)
		{
			return inventory.EquippedPassiveOrb == EInventoryOrbType.Pink;
		}
		return true;
	}

	public override void Update(float delta)
	{
		if (_timeWithoutBeingTriggered < 0.1f)
		{
			_timeWithoutBeingTriggered += delta;
		}
		base.Update(delta);
	}

	protected override void UpdateDoorState(float delta)
	{
		switch (_doorState)
		{
		case ESlidingDoorState.Opened:
			_openCloseTimer = 0f;
			if (_closeTimer > 0f)
			{
				_closeTimer -= delta;
				if (_closeTimer < 0f)
				{
					CloseAndLock();
				}
			}
			break;
		case ESlidingDoorState.Opening:
		{
			if (_openCloseTimer <= 0f)
			{
				CreateRippleAnimation();
				_mainGem.IsGlowing = false;
				_mainGem.DrawColor = Color.White * 0f;
			}
			_openCloseTimer += delta;
			float num = _openCloseTimer / 0.5f;
			if (num >= 1f)
			{
				SetPositionToOpen();
				_doorState = ESlidingDoorState.Opened;
				EmitOpeningParticles(_baseY);
				EmitOpeningParticles(_baseY - 80);
			}
			else
			{
				num = (float)(1.0 - Math.Cos(num * ((float)Math.PI / 2f)));
				int num3 = (int)(num * 48f);
				Position = new Point(_position.X, _baseY + num3);
				_topHalf.Position = new Point(_position.X, _baseY - 40 - num3);
			}
			break;
		}
		case ESlidingDoorState.Falling:
		{
			_openCloseTimer += delta;
			float num = _openCloseTimer / 0.39f;
			if (num >= 1f)
			{
				Position = new Point(_position.X, _baseY);
				_topHalf.Position = new Point(_position.X, _baseY - 40);
				_doorState = ESlidingDoorState.Closed;
				base.TriggerBbox = new Rectangle(base.TriggerBbox.X, base.TriggerBbox.Y, 16, 80);
				EmitClosedParticles();
				CreateRippleAnimation();
				_mainGem.IsGlowing = true;
				_mainGem.DrawColor = Color.White;
			}
			else
			{
				num = (float)Math.Cos(num * ((float)Math.PI / 2f));
				int num2 = (int)(num * 48f);
				Position = new Point(_position.X, _baseY + num2);
				_topHalf.Position = new Point(_position.X, _baseY - 40 - num2);
			}
			break;
		}
		case ESlidingDoorState.Closed:
		{
			_openCloseTimer = 0f;
			_glowTimer += delta;
			float num = (float)((Math.Sin(_glowTimer * 2f) + 1.0) / 4.0) + 0.5f;
			Color baseGemGlowColor = _baseGemGlowColor;
			baseGemGlowColor.A = (byte)(num * 255f);
			_mainGem.GlowColor = baseGemGlowColor;
			{
				foreach (Appendage gem in _gems)
				{
					gem.GlowColor = baseGemGlowColor;
				}
				break;
			}
		}
		}
	}

	private void CreateRippleAnimation()
	{
		_rippleAnimation = new BattleAnimation(_sprite, _mainGem.Bbox.Center.Add(1, 0), _level)
		{
			AnimationStart = 5,
			AnimationLength = 4,
			DoesFadeOut = true
		};
		AddBattleAnimation(_rippleAnimation);
	}

	internal override void SetPositionToOpen()
	{
		Position = new Point(_position.X, _baseY + 48);
		_topHalf.Position = new Point(_position.X, _baseY - 80);
		base.TriggerBbox = new Rectangle(base.TriggerBbox.X, base.TriggerBbox.Y, 16, 16);
	}

	protected override void EmitOpeningParticles(int doorTop)
	{
		AddDustParticle(new Point(Bbox.Center.X, doorTop));
	}

	protected override void EmitClosedParticles()
	{
		AddDustParticle(new Point(Position.X, Position.Y - 40));
	}

	private void AddDustParticle(Point targetPoint)
	{
		BattleAnimation battleAnimation = BattleAnimation.Create(EBattleAnimationType.CrackingDust, targetPoint, ETeamSide.Neutral, IsFacingLeft, _level, doesPlaySFX: false);
		battleAnimation.DrawPlane = EDrawPlane.Normal;
		_level.AddAnimation(battleAnimation);
	}

	private void DoExitScript()
	{
		Console.WriteLine($"[TransitionDoor] DoExitScript triggered at {Position}, Level {_level.ID}, Room {_level.RoomID}");
		_mainGem.IsGlowing = false;
		_mainGem.DrawColor = Color.White * 0f;
		_level.MainHero.TeleportToPoint(_startingPosition);
		_level.MainHero.StopMovement();
		_level.InstantUpdateCamera();
		PlayCue(ESFX.DoorTransitionClose, Position);
		OpenDoor(-1f);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Run, 0f, 0.2f, new Vector4((!IsFacingLeft) ? 1 : (-1), 0f, 0f, 0f))
		{
			DoesBlockQueue = true
		});
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.5f, Vector4.Zero));
		_doorState = ESlidingDoorState.Falling;
		_timeWithoutBeingTriggered = 10f;
		if (_isRoyalDoor)
		{
			return;
		}
		EBGM eBGM = Level.GetLevelSong(_level.ID, _level.RoomID, _level.GameSave);
		Jukebox jukeBox = _level.JukeBox;
		if (_level.ID == 10 && _level.RoomID == 0 && !_level.GameSave.GetSaveBool("IsPastCleared"))
		{
			eBGM = EBGM.None;
		}
		if (jukeBox.CurrentSongEnum == eBGM)
		{
			jukeBox.FadeInSong(0.5f);
			return;
		}
		if (_level.ID != 15)
		{
			AddWaitScript(0.2f);
		}
		_level.AddScript(new ScriptAction(eBGM, shouldForceRestart: true, shouldStopPreviousSong: true));
	}

	public override void DoOpenScript()
	{
		Console.WriteLine($"[TransitionDoor] DoOpenScript triggered at {Position}, Level {_level.ID}, Room {_level.RoomID}");
		_doorState = ESlidingDoorState.Opening;
		_level.PlayCue(ESFX.DoorTransitionOpen, Position);
		_level.TogglePlayerIsInvulnerable(isInvulnerable: true);
		if (!_isRoyalDoor)
		{
			_level.AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				SleepTime = 0.55f,
				Delegate = delegate
				{
					_level.JukeBox.FadeOutSong(3f);
				}
			});
		}
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.5f, Vector4.Zero)
		{
			DoesBlockQueue = true
		});
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Run, 0f, 0.15f, new Vector4(IsFacingLeft ? 1 : (-1), 0f, 0f, 0f))
		{
			DoesBlockQueue = true
		});
		_level.AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = MakePlayerNotInvulnerable
		});
	}

	private void MakePlayerNotInvulnerable()
	{
		_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
	}
}
