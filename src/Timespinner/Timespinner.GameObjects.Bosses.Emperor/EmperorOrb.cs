using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorOrb : Animate
{
	private const int FloorY = 208;

	private const int CeilingY = 16;

	private const int LeftWallX = 16;

	private const int RightWallX = 560;

	private const int BladeCount = 5;

	private const int BlueProjectileCount = 2;

	private const int OrbBboxRadius = 12;

	private const int OrbOrbitRadius = 64;

	private const int AnchorOffsetX = 8;

	private const int AnchorOffsetY = -40;

	private const float OrbOrbitFrequency = 4f;

	private const float OrbOuterRotationFrequency = 1f / (2f * (float)Math.PI);

	private const float EmpireTimeBetweenPunches = 0.4f;

	private const float PlasmaTimeBeforeZapping = 0.5f;

	private const float TimeToShowBlades = 0.2f;

	private const float TimeToHideBlades = 0.15f;

	private const float BladeSpinRate = 15f;

	private readonly int _baseDamage;

	private readonly int _orbIndex;

	private readonly float _baseOscillationOffset;

	private readonly EmperorOrbOrbitState _baseOrbitState;

	private readonly EmperorBlueProjectile[] _blueProjectiles = new EmperorBlueProjectile[2];

	private readonly Appendage[] _bladeAppendages = new Appendage[5];

	private readonly Queue<EmperorOrbOrbitState> _orbStateQueue = new Queue<EmperorOrbOrbitState>();

	private bool _isAnchorFacingLeft;

	private bool _isShowingBlades;

	private bool _isHidingBlades;

	private EEmperorOrbType _orbType;

	private int _changeInAnchorPositionX;

	private float _specialAttackTimer;

	private float _bladesShowTimer;

	private Point _anchorPosition;

	private EmperorOrbOrbitState _activeOrbitState;

	private EmperorOrbOrbitState _lastOrbitState;

	private SFXCueInstance _spinningLoopCueInstance;

	internal bool IsOnFrontPlane { get; private set; }

	internal EEmperorOrbType OrbType => _orbType;

	internal Color TrailColor => _trailColor;

	public EmperorOrb(Point inPosition, Level inLevel, int inID, SpriteSheet sprite, EEmperorOrbType orbType, int baseDamage, int orbIndex, bool isViletianEmperor)
		: base(inPosition, inLevel, inID)
	{
		_sprite = sprite;
		_orbType = orbType;
		_baseDamage = baseDamage;
		_orbIndex = orbIndex;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 12, 12);
		DrawOrigin = new Vector2(6f, 6f);
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = true;
		_isFlying = true;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_trailLength = 75;
		_brushTrailSize = 14;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 15;
		_trailShrinkRate = 0.00033f;
		_baseOscillationOffset = (float)Math.PI * 2f / 3f * (float)(1 + inID);
		EmperorOrbOrbitSpecification spec = new EmperorOrbOrbitSpecification
		{
			Radius = 64,
			Frequency = 4f,
			IsRotatingOnZAxis = true,
			IsTilted = true,
			ZRotationFrequency = 1f / (2f * (float)Math.PI),
			TransitionTime = 0.5f,
			OrbOrbitType = EEmperorOrbOrbitType.Default
		};
		_baseOrbitState = new EmperorOrbOrbitState(spec, _baseOscillationOffset, 0)
		{
			IsOnFrontPlane = (inID <= 1)
		};
		for (int i = 0; i < 5; i++)
		{
			Appendage appendage = new Appendage(this, new Point(1, 1), new Point(10, 47), _level, _sprite)
			{
				DrawOrigin = new Vector2(10f, 47f),
				Rotation = (float)Math.PI * 2f / 5f * (float)i,
				AnchorObject = this,
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(0, -6),
				DoesInheritDrawColor = false
			};
			appendage.ChangeAnimation(-1);
			_bladeAppendages[i] = appendage;
			_appendages.Add(appendage);
		}
		_activeOrbitState = _baseOrbitState;
		ChangeOrbFramByType(_orbType);
		for (int j = 0; j < 2; j++)
		{
			_blueProjectiles[j] = new EmperorBlueProjectile(_level, _baseDamage, isViletianEmperor);
		}
	}

	internal void ChangeOrbType(EEmperorOrbType newType)
	{
		_orbType = newType;
		ChangeOrbFramByType(_orbType);
		AddOrbChangeAnimation(shouldGiveToLevel: false);
	}

	internal void AddOrbChangeAnimation(bool shouldGiveToLevel)
	{
		BattleAnimation battleAnimation = new BattleAnimation(_sprite, Bbox.Center, _level);
		battleAnimation.AnchorObject = this;
		battleAnimation.AnchorOffset = new Point(0, -6);
		battleAnimation.AnimationStart = 28;
		battleAnimation.AnimationLength = 4;
		battleAnimation.AnimationSpeed = 0.07f;
		battleAnimation.TeamSide = ETeamSide.Enemies;
		BattleAnimation newAnimation = battleAnimation;
		if (shouldGiveToLevel)
		{
			_level.AddAnimation(newAnimation);
		}
		else
		{
			AddBattleAnimation(newAnimation);
		}
	}

	private void ChangeOrbFramByType(EEmperorOrbType orbType)
	{
		switch (orbType)
		{
		case EEmperorOrbType.Blue:
			_trailColor = new Color(0.15f, 0.15f, 0.75f, 0.05f);
			ChangeAnimation(27);
			break;
		case EEmperorOrbType.Empire:
			_trailColor = new Color(0.65f, 0.4f, 0.8f, 0.05f);
			ChangeAnimation(25);
			break;
		case EEmperorOrbType.Fire:
			_trailColor = new Color(0.75f, 0.5f, 0.25f, 0.1f);
			ChangeAnimation(26);
			break;
		case EEmperorOrbType.Plasma:
			_trailColor = new Color(0.75f, 0.25f, 0.5f, 0.1f);
			ChangeAnimation(25);
			break;
		case EEmperorOrbType.Blade:
			_trailColor = new Color(0.35f, 0.75f, 0.3f, 0.1f);
			ChangeAnimation(26);
			break;
		case EEmperorOrbType.Iron:
			_trailColor = new Color(0.75f, 0.75f, 0.75f, 0.1f);
			ChangeAnimation(27);
			break;
		}
	}

	internal void Update(float delta, Point anchorLocation, bool isAnchorFacingLeft)
	{
		_isAnchorFacingLeft = isAnchorFacingLeft;
		Point anchorPosition = _anchorPosition;
		_anchorPosition = anchorLocation.Add(8 * (_isAnchorFacingLeft ? 1 : (-1)), -40);
		_changeInAnchorPositionX = _anchorPosition.X - anchorPosition.X;
		UpdateOrbStates(delta);
		UpdateBlades(delta);
		Update(delta);
	}

	private void UpdateOrbStates(float delta)
	{
		Point orbitPosition = _anchorPosition;
		UpdateActiveOrbState(delta);
		if (_lastOrbitState != null)
		{
			_lastOrbitState.Update(delta);
			IsOnFrontPlane = _lastOrbitState.IsOnFrontPlane;
			orbitPosition = _lastOrbitState.GetOrbPosition(orbitPosition, _anchorPosition, _isAnchorFacingLeft);
		}
		else
		{
			IsOnFrontPlane = _activeOrbitState.IsOnFrontPlane;
		}
		Position = _activeOrbitState.GetOrbPosition(orbitPosition, _anchorPosition, _isAnchorFacingLeft);
		if (_activeOrbitState.IsFinished)
		{
			if (_orbStateQueue.Count > 0)
			{
				_lastOrbitState = _activeOrbitState;
				_activeOrbitState = _orbStateQueue.Dequeue();
				DeactivateOrbitState(_lastOrbitState);
				ActivateNewOrbitState(_activeOrbitState);
			}
			else if (_activeOrbitState == _baseOrbitState && _baseOrbitState.IsTransitioned)
			{
				_lastOrbitState = null;
			}
			else
			{
				_lastOrbitState = _activeOrbitState;
				_activeOrbitState = _baseOrbitState;
				_baseOrbitState.ResetTimers();
				DeactivateOrbitState(_lastOrbitState);
			}
		}
	}

	private void UpdateActiveOrbState(float delta)
	{
		_activeOrbitState.Update(delta);
		if (_activeOrbitState.OrbOrbitType == EEmperorOrbOrbitType.EmpireAttack)
		{
			_specialAttackTimer += delta;
			if (_specialAttackTimer > 0.4f)
			{
				Point inPosition = Position.Add(0, -18);
				_level.AddProjectile(new EmperorEmpireMeleeDamageArea(_level, inPosition, null, _isAnchorFacingLeft, _changeInAnchorPositionX, _baseDamage, _orbIndex % 2 == 0));
				_specialAttackTimer -= 0.4f;
				PlayCue(ESFX.LunaisOrbEmpireMelee);
			}
		}
		else if (_activeOrbitState.OrbOrbitType == EEmperorOrbOrbitType.BlueAttack || _activeOrbitState.OrbOrbitType == EEmperorOrbOrbitType.FireAttack)
		{
			if (!_activeOrbitState.IsAtApex)
			{
				return;
			}
			PlayCue(ESFX.BossEmperorEnergyBall);
			Point position = Position.Add(0, -4);
			EmperorBlueProjectile emperorBlueProjectile = null;
			EmperorBlueProjectile[] blueProjectiles = _blueProjectiles;
			foreach (EmperorBlueProjectile emperorBlueProjectile2 in blueProjectiles)
			{
				if (emperorBlueProjectile2.IsFinished)
				{
					emperorBlueProjectile = emperorBlueProjectile2;
					break;
				}
			}
			if (emperorBlueProjectile != null)
			{
				emperorBlueProjectile.Reset(position, _isAnchorFacingLeft);
				_level.AddProjectile(emperorBlueProjectile);
			}
		}
		else
		{
			if (_activeOrbitState.OrbOrbitType != EEmperorOrbOrbitType.PlasmaAttack || !(_specialAttackTimer < 0.5f))
			{
				return;
			}
			_specialAttackTimer += delta;
			if (!(_specialAttackTimer >= 0.5f))
			{
				return;
			}
			Point center = Bbox.Center;
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
			_targetPosition = new Point(nearestProtagonistPosition.X, nearestProtagonistPosition.Y - 8);
			int num = (Position.X - _targetPosition.X) / 10;
			int num2 = (Position.Y - _targetPosition.Y) / 10;
			int num3 = _targetPosition.X;
			int num4 = _targetPosition.Y;
			if (num != 0 || num2 != 0)
			{
				while (num3 > 16 && num3 < 560 && num4 > 16 && num4 < 208)
				{
					num3 -= num;
					num4 -= num2;
				}
			}
			if (num3 < 16)
			{
				num3 = 16;
			}
			else if (num3 > 560)
			{
				num3 = 560;
			}
			if (num4 < 16)
			{
				num4 = 16;
			}
			else if (num4 > 208)
			{
				num4 = 208;
			}
			_targetPosition = new Point(num3, num4);
			List<Vector4> intervalsBetween = ThunderBoltDamageArea.GetIntervalsBetween(center, _targetPosition, _level);
			int inDamage = (int)Math.Ceiling((float)_baseDamage * 1.1f);
			ThunderBoltDamageArea thunderBoltDamageArea = new ThunderBoltDamageArea(_level, center, ETeamSide.Enemies, inDamage, intervalsBetween, _isAnchorFacingLeft, null, EThunderBoltType.VolTerrilis);
			PlayCue(ESFX.BossVolTerrilisLightningCast);
			thunderBoltDamageArea.MakePreBolt();
			_level.AddProjectile(thunderBoltDamageArea);
		}
	}

	private void ActivateNewOrbitState(EmperorOrbOrbitState newState)
	{
		if (newState == null)
		{
			return;
		}
		switch (newState.OrbOrbitType)
		{
		case EEmperorOrbOrbitType.BlueAttack:
			_level.AddProjectile(new EmperorBlueMeleeDamageArea(_level, Bbox.Center, this, _baseDamage));
			break;
		case EEmperorOrbOrbitType.FireAttack:
			_level.AddProjectile(new EmperorFireMeleeDamageArea(_level, Bbox.Center, this, _baseDamage));
			break;
		case EEmperorOrbOrbitType.BladeAttack:
		case EEmperorOrbOrbitType.IronAttack:
		{
			ESFX cue;
			if (newState.OrbOrbitType == EEmperorOrbOrbitType.BladeAttack)
			{
				_level.AddProjectile(new EmperorBladeMeleeDamageArea(_level, Bbox.Center, this, _baseDamage));
				PlayCue(ESFX.BossEmperorBladeStart);
				cue = ESFX.BossEmperorBladesLoop;
			}
			else
			{
				_level.AddProjectile(new EmperorIronMeleeDamageArea(_level, Bbox.Center, this, _baseDamage));
				PlayCue(ESFX.BossVolTerrilisIronStart);
				cue = ESFX.BossVolTerrilisIronLoop;
			}
			if (_spinningLoopCueInstance == null)
			{
				_spinningLoopCueInstance = PlayCue(cue, isLooped: true);
			}
			else if (!_spinningLoopCueInstance.IsFinished)
			{
				_spinningLoopCueInstance.Resume();
			}
			ShowBlades();
			break;
		}
		case EEmperorOrbOrbitType.EmpireAttack:
			_specialAttackTimer = (float)(-base.ID) * 0.1667f;
			break;
		case EEmperorOrbOrbitType.PlasmaAttack:
			_specialAttackTimer = (float)(-base.ID) * 0.1667f;
			break;
		}
	}

	private void CancelOrbState()
	{
		if (_activeOrbitState != null)
		{
			_activeOrbitState.Cancel();
		}
	}

	private void DeactivateOrbitState(EmperorOrbOrbitState oldState)
	{
		if (oldState == null)
		{
			return;
		}
		switch (oldState.OrbOrbitType)
		{
		case EEmperorOrbOrbitType.BladeAttack:
		case EEmperorOrbOrbitType.IronAttack:
			HideBlades();
			if (_spinningLoopCueInstance != null)
			{
				_spinningLoopCueInstance.Pause(0.25f);
			}
			break;
		case EEmperorOrbOrbitType.EmpireAttack:
			break;
		}
	}

	private void ShowBlades()
	{
		_isShowingBlades = true;
		_isHidingBlades = false;
		_bladesShowTimer = 0f;
		Appendage[] bladeAppendages = _bladeAppendages;
		foreach (Appendage appendage in bladeAppendages)
		{
			appendage.IsFacingLeft = _isAnchorFacingLeft;
			appendage.ChangeAnimation(32);
		}
	}

	private void HideBlades()
	{
		_isHidingBlades = true;
		_bladesShowTimer = 0f;
	}

	private void UpdateBlades(float delta)
	{
		if (!_isShowingBlades)
		{
			return;
		}
		Color drawColor = Color.White;
		float num = 15f * delta;
		if (!_isHidingBlades)
		{
			if (_bladesShowTimer < 0.2f)
			{
				_bladesShowTimer += delta;
				if (_bladesShowTimer < 0.2f)
				{
					drawColor = Color.Transparent.SineInterpolate(Color.White, _bladesShowTimer / 0.2f);
				}
			}
		}
		else
		{
			_bladesShowTimer += delta;
			if (_bladesShowTimer < 0.15f)
			{
				drawColor = Color.White.CosInterpolate(Color.Transparent, _bladesShowTimer / 0.15f);
			}
			else
			{
				_isShowingBlades = false;
				_bladesShowTimer = 0f;
				drawColor = Color.Transparent;
				Appendage[] bladeAppendages = _bladeAppendages;
				foreach (Appendage appendage in bladeAppendages)
				{
					appendage.ChangeAnimation(-1);
				}
			}
		}
		Appendage[] bladeAppendages2 = _bladeAppendages;
		foreach (Appendage appendage2 in bladeAppendages2)
		{
			appendage2.Rotation += num;
			if (appendage2.Rotation >= (float)Math.PI * 2f)
			{
				appendage2.Rotation -= (float)Math.PI * 2f;
			}
			appendage2.DrawColor = drawColor;
		}
	}

	internal void ReturnToBaseOrbit()
	{
		_lastOrbitState = _activeOrbitState;
		_activeOrbitState = _baseOrbitState;
		_activeOrbitState.ResetTimers();
	}

	internal void AddOrbState(EmperorOrbOrbitSpecification newStateSpec, float delay)
	{
		EmperorOrbOrbitState emperorOrbOrbitState = new EmperorOrbOrbitState(newStateSpec, _baseOscillationOffset, _orbIndex);
		emperorOrbOrbitState.DurationTime += delay;
		if (_activeOrbitState.IsFinished)
		{
			_lastOrbitState = _activeOrbitState;
			_activeOrbitState = emperorOrbOrbitState;
			DeactivateOrbitState(_lastOrbitState);
			ActivateNewOrbitState(emperorOrbOrbitState);
		}
		else
		{
			_orbStateQueue.Enqueue(emperorOrbOrbitState);
		}
	}

	public override void Kill()
	{
		if (_activeOrbitState != null && _activeOrbitState.OrbOrbitType == EEmperorOrbOrbitType.Death)
		{
			_activeOrbitState.ResetTimers();
		}
		else
		{
			CancelOrbState();
			_orbStateQueue.Clear();
			AddOrbState(new EmperorOrbOrbitSpecification
			{
				OrbOrbitType = EEmperorOrbOrbitType.Death,
				DurationTime = 10f,
				Radius = 64,
				Frequency = 4f,
				IsRotatingOnZAxis = true,
				IsTilted = true,
				ZRotationFrequency = 1f / (2f * (float)Math.PI),
				TransitionTime = 0.5f
			}, 0f);
		}
		_doesDrawBaseSprite = true;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_trailLength = 75;
		_trailColor = Color.White;
		base.DrawColor = Color.White;
	}
}
