using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CavesSnail : Monster
{
	private enum ECavesSnailAIState
	{
		Idle,
		Spit,
		Hiding,
		Unhiding,
		Hidden,
		ShootingHarpoon
	}

	private const int SpitOriginOffsetX = 19;

	private const int SpitOriginOffsetY = -21;

	private const int HideDistanceThresholdX = 200;

	private const float TimeToHide = 1f;

	private const float TimeToUnhide = 1f;

	private const float TimeToSpit = 2f;

	private const float IndefiniteActionTime = 1000f;

	private const float TimeBeforeStartingFadeOut = 0.5f;

	private const float TimeToFadeOut = 0.75f;

	private const float TimeForEntireFade = 1.25f;

	private const int SpitStartOffsetX = 92;

	private const int SpitStartOffsetY = -40;

	private const int SpitEndOffsetX = 90;

	private const int SpitEndOffsetY = -34;

	private const float StartSpitSpeed = 400f;

	private const float EndSpitSpeed = 50f;

	private const float SpitStartingAngle = -(float)Math.PI / 8f;

	private const float SpitEndingAngle = (float)Math.PI / 8f;

	private const float TimeToGoopSpits = 0.25f;

	private const float TimeBetweenIndividualSpits = 0.033f;

	private const int HarpoonArmLinkCount = 16;

	private const int HarpoonShootDistance = 128;

	private const int HarpoonShootHeight = 10;

	private const float TimeForHarpoonToJigger = 1f;

	private const float TimeForHarpoonOut = 0.3f;

	private const float TimeForHarpoonToFall = 0.1f;

	private const float TimeForHarpoonWait = 0.5f;

	private const float TimeForHarpoonReturn = 0.75f;

	private const float TimeBeforeHarpoonWaits = 0.4f;

	private const float TimeBeforeHarpoonReturns = 0.90000004f;

	private const float TimeForEntireHarpoonSequence = 1.65f;

	private const float TimeToShootHarpoon = 2.65f;

	private const float TimeBetweenDamageDeflectionAnimations = 0.1f;

	private static readonly Color FadedColor = new Color(0.675f, 0.7f, 0.65f);

	private readonly Appendage _shellAppendage;

	private readonly Appendage _farShellAppendage;

	private readonly Appendage _torsoAppendage;

	private readonly Appendage _headAppendage;

	private readonly Appendage _harpoonAppendage;

	private readonly CharacterSequenceSpecification _unhideSequence;

	private readonly CharacterSequenceSpecification _hideSequence;

	private readonly CharacterSequenceSpecification _spitSequence;

	private readonly CharacterSequenceSpecification _shootHarpoonSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly CavesSnailSpitDamageArea _spitDamageArea;

	private bool _isCreatingSpits;

	private bool _isShootingToTheLeft;

	private bool _isShootingHarpoon;

	private ECavesSnailAIState _globalState;

	private ECavesSnailAIState _lastNonIdleGlobalState;

	private float _spitCreationTimer;

	private float _individualSpitTimer;

	private float _currentGlobalStateTimer;

	private float _harpoonTimer;

	private float _damageDeflectionTimer;

	private Point _spitCreationPosition;

	public CavesSnail(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_bboxOffset = new Point(0, 0);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		base.CannotBeGrabbed = true;
		base.DoesTouchDamageKnockback = true;
		_doesDrawBaseSprite = false;
		_timeToTurnAround = 0f;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		int baseDamage = (int)Math.Ceiling((float)base.Damage * 0.9f);
		_spitDamageArea = new CavesSnailSpitDamageArea(_level, Position, ETeamSide.Enemies, baseDamage, _sprite, Position.Y);
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 4)
		{
			_unhideSequence = base.CharacterSpecification.Sequences[0];
			_hideSequence = base.CharacterSpecification.Sequences[1];
			_spitSequence = base.CharacterSpecification.Sequences[2];
			_deathSequence = base.CharacterSpecification.Sequences[3];
			_shootHarpoonSequence = base.CharacterSpecification.Sequences[4];
		}
		if (base.Appendages.Count <= 2)
		{
			return;
		}
		_shellAppendage = base.Appendages[0];
		_farShellAppendage = base.Appendages[1];
		_torsoAppendage = base.Appendages[2];
		_headAppendage = _torsoAppendage.Appendages[0];
		_harpoonAppendage = _torsoAppendage.Appendages[1];
		_harpoonAppendage.AddLinks(16, EAppendageFollowType.LinearInteroplate, new Rectangle(Position.X, Position.Y, 10, 10), new Point(1, 1), 1, Point.Zero);
		_harpoonAppendage.DoesDrawAppendagesInReverse = true;
		foreach (Appendage appendage in _harpoonAppendage.Appendages)
		{
			appendage.DoesCollideWithAnything = false;
		}
	}

	public override void InitializeMob()
	{
		if (_level.MainHero != null && _level.MainHero.Position.X > Position.X)
		{
			IsFacingLeft = false;
		}
		_globalState = ECavesSnailAIState.Hidden;
		UpdateCustomScriptAIAction(0f);
		Update(0f);
		UpdateAppendages(0f);
		base.InitializeMob();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_damageDeflectionTimer > 0f)
			{
				_damageDeflectionTimer -= delta;
			}
			if (_isCreatingSpits)
			{
				if (_spitCreationTimer <= 0f)
				{
					_spitDamageArea.Reset(_spitCreationPosition);
					_level.AddProjectile(_spitDamageArea);
				}
				_spitCreationTimer += delta;
				if (_spitCreationTimer >= 0.25f)
				{
					EndSpits();
				}
				else
				{
					_individualSpitTimer -= delta;
					if (_individualSpitTimer <= 0f)
					{
						_individualSpitTimer = 0.033f;
						float percentage = _spitCreationTimer / 0.25f;
						CreateSpit(percentage);
					}
				}
			}
		}
		base.Update(delta);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentGlobalStateTimer = 0f;
		_currentAction = EAIAction.Custom;
		Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(_headAppendage.Position);
		bool flag = nearestProtagonistPosition.X < Position.X == IsFacingLeft && Math.Abs(nearestProtagonistPosition.X - Position.X) < 200;
		_nextActionTimer = 1000f;
		if (!flag)
		{
			_globalState = ((_globalState == ECavesSnailAIState.Idle) ? ECavesSnailAIState.Hiding : ECavesSnailAIState.Hidden);
		}
		else if (_globalState == ECavesSnailAIState.Hidden)
		{
			_globalState = ECavesSnailAIState.Unhiding;
		}
		else
		{
			if (_level.NextRandomInt(0, 1) == 0)
			{
				_globalState = ECavesSnailAIState.ShootingHarpoon;
			}
			else
			{
				_globalState = ECavesSnailAIState.Spit;
			}
			if (_globalState == ECavesSnailAIState.Spit && _lastNonIdleGlobalState == ECavesSnailAIState.Spit)
			{
				_globalState = ECavesSnailAIState.ShootingHarpoon;
			}
		}
		_lastNonIdleGlobalState = _globalState;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		switch (_globalState)
		{
		case ECavesSnailAIState.Idle:
			_headAppendage.ChangeAnimation(10);
			if (_currentGlobalStateTimer > 0.5f)
			{
				FinishAttack();
			}
			break;
		case ECavesSnailAIState.Hiding:
			if (_currentGlobalStateTimer <= 0f)
			{
				_headAppendage.PlayCue(ESFX.EnemySnailMoveIn);
				SetCharacterSequence(_hideSequence);
			}
			else if (_currentGlobalStateTimer >= 1f)
			{
				_globalState = ECavesSnailAIState.Hidden;
				FinishAttack();
			}
			break;
		case ECavesSnailAIState.Hidden:
			if (_currentGlobalStateTimer > 1f)
			{
				FinishAttack();
			}
			break;
		case ECavesSnailAIState.Unhiding:
			if (_currentGlobalStateTimer <= 0f)
			{
				_headAppendage.PlayCue(ESFX.EnemySnailMoveOut);
				SetCharacterSequence(_unhideSequence);
			}
			else if (_currentGlobalStateTimer >= 1f)
			{
				_globalState = ECavesSnailAIState.Idle;
				FinishAttack();
			}
			break;
		case ECavesSnailAIState.Spit:
			UpdateSpitAttack();
			break;
		case ECavesSnailAIState.ShootingHarpoon:
			UpdateShootingHarpoonAttack(delta);
			break;
		}
		_currentGlobalStateTimer += delta;
	}

	private void UpdateSpitAttack()
	{
		if (_currentGlobalStateTimer <= 2f)
		{
			if (_currentGlobalStateTimer <= 0f)
			{
				_headAppendage.PlayCue(ESFX.EnemySnailSpitPrep);
				SetCharacterSequence(_spitSequence);
			}
		}
		else
		{
			_globalState = ECavesSnailAIState.Idle;
			FinishAttack();
		}
	}

	private void UpdateShootingHarpoonAttack(float delta)
	{
		if (_currentGlobalStateTimer <= 2.65f)
		{
			if (_currentGlobalStateTimer <= 0f)
			{
				_harpoonAppendage.PlayCue(ESFX.EnemySnailHarpoonPrep);
				SetCharacterSequence(_shootHarpoonSequence);
				_harpoonTimer = 0f;
			}
			if (!_isShootingHarpoon)
			{
				return;
			}
			float harpoonTimer = _harpoonTimer;
			_harpoonTimer += delta;
			_harpoonAppendage.FollowType = EAppendageFollowType.None;
			Point point = new Point(_torsoAppendage.Position.X, _torsoAppendage.Position.Y - 10);
			Point point2 = Point.Zero;
			if (_harpoonTimer <= 0.3f)
			{
				float num = (float)Math.Sin((float)Math.PI / 2f * _harpoonTimer / 0.3f);
				point2 = new Point((int)Math.Ceiling(128f * num), 0);
				if (harpoonTimer <= 0f)
				{
					_harpoonAppendage.PlayCue(ESFX.EnemySnailHarpoon);
				}
			}
			else if (_harpoonTimer < 0.4f)
			{
				float num2 = (float)Math.Sin((float)Math.PI / 2f * (_harpoonTimer - 0.3f) / 0.1f);
				point2 = new Point(128, (int)(10f * num2));
			}
			else if (_harpoonTimer < 0.90000004f)
			{
				point2 = new Point(128, 10);
			}
			else if (_harpoonTimer < 1.65f)
			{
				if (harpoonTimer < 0.90000004f)
				{
					_harpoonAppendage.PlayCue(ESFX.EnemySnailHarpoonRetract);
				}
				float num3 = 1f - (_harpoonTimer - 0.90000004f) / 0.75f;
				point2 = new Point((int)(128f * num3), (int)(10f * num3));
			}
			_harpoonAppendage.Position = new Point(point.X + point2.X * ((!IsFacingLeft) ? 1 : (-1)), point.Y + point2.Y);
		}
		else
		{
			_harpoonAppendage.FollowType = EAppendageFollowType.AnchorLocked;
			_harpoonAppendage.AnchorOffset = new Point(0, -10);
			_globalState = ECavesSnailAIState.Idle;
			_isShootingHarpoon = false;
			FinishAttack();
		}
	}

	private void FinishAttack()
	{
		FinishAttack(1f);
	}

	private void FinishAttack(float waitTime)
	{
		_nextActionTimer = waitTime;
		_currentAction = EAIAction.Idle;
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		switch (specification.IntArgument)
		{
		case 0:
		{
			float num = ((!IsFacingLeft) ? 1 : (-1));
			Point position = _headAppendage.Position;
			position.X += (int)(19f * num);
			position.Y += -21;
			StartSpittingVomit();
			PlayCue(ESFX.EnemySnailSpit, position);
			break;
		}
		case 1:
			PlayCue(ESFX.EnemySnailDeath);
			break;
		case 2:
			_isShootingHarpoon = true;
			break;
		case 3:
		{
			Point inPosition = new Point(IsFacingLeft ? _harpoonAppendage.Bbox.Left : _harpoonAppendage.Bbox.Right, _harpoonAppendage.Bbox.Center.Y);
			_level.AddAnimation(new BattleAnimation(_sprite, inPosition, _level)
			{
				TeamSide = ETeamSide.Enemies,
				AnimationStart = 26,
				AnimationLength = 6
			});
			break;
		}
		}
	}

	private void StartSpittingVomit()
	{
		_isCreatingSpits = true;
		_spitCreationTimer = 0f;
		_isShootingToTheLeft = IsFacingLeft;
	}

	private void CreateSpit(float percentage)
	{
		float num = MathHelper.Lerp(-(float)Math.PI / 8f, (float)Math.PI / 8f, percentage);
		float num2 = (float)Math.Cos(num) * (float)((!_isShootingToTheLeft) ? 1 : (-1));
		float num3 = (float)Math.Sin(num);
		float num4 = MathHelper.Lerp(400f, 50f, percentage);
		Vector2 iV = new Vector2(num2 * num4, num3 * num4);
		int num5 = (int)MathHelper.Lerp(92f, 90f, percentage);
		int y = (int)MathHelper.Lerp(-40f, -34f, percentage);
		_spitCreationPosition = Position.Add(new Point(num5 * ((!_isShootingToTheLeft) ? 1 : (-1)), y));
		_spitDamageArea.EmitSpit(_spitCreationPosition, iV);
	}

	private void EndSpits()
	{
		_isCreatingSpits = false;
		_spitCreationTimer = 0f;
		_individualSpitTimer = 0f;
		_spitDamageArea.End();
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = false;
		bool flag2 = false;
		if (_globalState != ECavesSnailAIState.Hidden)
		{
			List<Rectangle> list = FindIntersectingBoundingBoxes(sourceRectangle, 3);
			if (list != null && list.Count > 0)
			{
				foreach (Rectangle item in list)
				{
					if (item != _shellAppendage.Bbox && item != _farShellAppendage.Bbox)
					{
						flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
						break;
					}
				}
				if (!flag)
				{
					flag2 = true;
				}
			}
		}
		else
		{
			flag2 = true;
		}
		if (flag2 && _damageDeflectionTimer <= 0f)
		{
			_level.AddAnimation(EBattleAnimationType.SmallFail, where, ETeamSide.Heroes, IsFacingLeft);
			_damageDeflectionTimer = 0.1f;
		}
		return flag;
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			DropLoot();
			SetCharacterSequence(_deathSequence);
			base.IsSolidWhenFrozen = false;
		}
		if (!(_deathScriptTimer < 1.25f))
		{
			return;
		}
		_deathScriptTimer += delta;
		if (_deathScriptTimer >= 0.5f)
		{
			float num = (_deathScriptTimer - 0.5f) / 0.75f;
			if (num > 1f)
			{
				num = 1f;
			}
			base.DrawColor = Color.White.SineInterpolate(FadedColor, num);
			if (_isShootingHarpoon && _harpoonAppendage.Position.Y < Position.Y)
			{
				_harpoonAppendage.Position = new Point(_harpoonAppendage.Position.X, (int)MathHelper.Lerp(_harpoonAppendage.Position.Y, Position.Y, num));
			}
		}
		UpdateCharacterSequences(delta);
		UpdateAppendages(delta);
		UpdateAnimation(delta);
	}
}
