using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabCurtainWinch : EnvironmentPrefabBase
{
	internal const string CurtainDrawbridgeSaveKey = "IsDrawbridgeRaised";

	internal const string CurtainWinchSaveKey = "HasWinchBeenUsed";

	private const int NumberOfLinks = 8;

	private const int LinkOffsetY = -7;

	private const float ChainOffsetMultiplier = 1f;

	private const float DistanceBetweenChains = 12f;

	private const float ChainLoopThreshold = 1000f;

	private const float LinkLoopThreshold = 96f;

	private const float RotationSpeed = 100f;

	private const float TimeToRotate = 3f;

	private const float TimeBeforeCoolingDown = 0.5f;

	private readonly Appendage _wheel;

	private readonly Appendage _chainHead;

	private readonly List<Appendage> _chainLinks = new List<Appendage>();

	private bool _isRotating;

	private bool _isRotatingClockwise;

	private bool _isDrawbridgeUp;

	private float _cooldownTimer;

	private float _rotateTimer;

	private float _changeInRotation;

	private float _chainOffset;

	private SFXCueInstance _chainRaiseCueInstance;

	public EnvPrefabCurtainWinch(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 32, 32);
		SnapBboxToPosition();
		_sprite = _level.GCM.SpMiscCurtain;
		_doesDrawBaseSprite = false;
		base.DrawPlane = EDrawPlane.Back;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isSolid = false;
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		_doAppendagesMatchImageFacing = false;
		if (base.CharacterSpecification == null || base.Appendages.Count <= 1)
		{
			return;
		}
		_chainHead = base.Appendages[0];
		_wheel = base.Appendages[1];
		for (int i = 0; i < 8; i++)
		{
			Appendage appendage = new Appendage(_chainHead, new Point(16, 4), new Point(0, 3), _level, _sprite);
			if (i % 2 == 0)
			{
				appendage.ChangeAnimation(8);
				appendage.DrawPriority = -1;
			}
			else
			{
				appendage.ChangeAnimation(9);
				appendage.BboxOffset = Point.Zero;
				appendage.DrawPriority = 1;
			}
			_chainLinks.Add(appendage);
			_chainHead.AddAppendage(appendage);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		bool levelSaveBool = _level.GetLevelSaveBool("IsDrawbridgeRaised");
		if (base.Level.GetLevelSaveBool("HasWinchBeenUsed"))
		{
			_isDrawbridgeUp = levelSaveBool;
		}
		else
		{
			_isDrawbridgeUp = !_level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(6);
		}
		_isRotatingClockwise = _isDrawbridgeUp;
		UpdateChain();
		Update(0f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_cooldownTimer > 0f)
			{
				_cooldownTimer -= delta;
			}
			if (_isRotating && _wheel != null && _chainHead != null)
			{
				UpdateWheelRotation(delta);
				UpdateChain();
			}
		}
		base.Update(delta);
	}

	private void UpdateWheelRotation(float delta)
	{
		_rotateTimer += delta;
		if (_rotateTimer > 3f)
		{
			_isRotating = false;
			if (_chainRaiseCueInstance != null)
			{
				_chainRaiseCueInstance.Stop(0.1f);
				_chainRaiseCueInstance = null;
			}
			PlayCue(_isDrawbridgeUp ? ESFX.FoleyDrawbridgeLockMid : ESFX.FoleyDrawbridgeLockProper);
		}
		_changeInRotation = delta * 100f * (float)(_isRotatingClockwise ? 1 : (-1));
		float rotation = _wheel.Rotation;
		rotation += _changeInRotation;
		if (_isRotatingClockwise)
		{
			if (rotation > (float)Math.PI * 2f)
			{
				rotation -= (float)Math.PI * 2f;
			}
		}
		else if (rotation < 0f)
		{
			rotation += (float)Math.PI * 2f;
		}
		_wheel.Rotation = rotation;
	}

	private void UpdateChain()
	{
		_chainOffset -= _changeInRotation * 1f;
		if (_chainOffset > 1000f)
		{
			_chainOffset -= 1000f;
		}
		else if (_chainOffset < 0f)
		{
			_chainOffset += 1000f;
		}
		int num = 0;
		Point center = _chainHead.Bbox.Center;
		foreach (Appendage chainLink in _chainLinks)
		{
			float num2 = (_chainOffset + (float)num * 12f) % 96f;
			chainLink.Position = new Point((int)((float)center.X - num2), center.Y + -7);
			num++;
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!base.IsFrozen && _cooldownTimer <= 0f && !_isRotating && who is Protagonist protagonist)
		{
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
			if (protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				DoSwitchActivate();
			}
		}
		return base.TriggerEvent(who, depth);
	}

	private void DoSwitchActivate()
	{
		_cooldownTimer = 0.5f;
		_isRotating = true;
		_rotateTimer = 0f;
		_isRotatingClockwise = !_isRotatingClockwise;
		_isDrawbridgeUp = !_isDrawbridgeUp;
		_chainRaiseCueInstance = PlayCue(_isDrawbridgeUp ? ESFX.FoleyDrawbridgeLowerLoop : ESFX.FoleyDrawbridgeRaiseLoop, isLooped: true);
		_level.SetLevelSaveBool("IsDrawbridgeRaised", _isDrawbridgeUp);
		_level.SetLevelSaveBool("HasWinchBeenUsed", value: true);
	}
}
