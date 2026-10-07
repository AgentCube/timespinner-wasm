using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CavesSporeVine : Monster
{
	public enum ECavesVineType
	{
		None,
		Left,
		Right,
		Bottom
	}

	private const int BaseSporeHeight = 97;

	private const int AttackingFrameA = 22;

	private const int AttackingFrameB = 27;

	private const float VineIdleAnimationSpeed = 0.15f;

	private const float VineSwipeAnimationSpeed = 0.05f;

	private const float SporeOscillationFrequency = 1.5f;

	private const float SporeOscillationRadius = 8f;

	private const float OscillationHeightMultiplier = 0.15f;

	private const float MomentumGainOnHit = 1f;

	private const float MomentumGainOnBeingAggroed = 5f;

	private const float MomentumDecayRate = 3f;

	private const float MaximumMomentum = 10f;

	private const float MomentumWidthMultiplier = 2f;

	private const float MomentumHeightMultiplier = 0.5f;

	private const float MomentumFrequencyMultiplier = 0.2f;

	private const float DeathSwayDecayRate = 0.1f;

	private static readonly Point VineSwipeAnimationBboxOffset = new Point(24, 8);

	private static readonly Point VineIdleAnimationBboxOffset = new Point(4, 0);

	private static readonly Point VineHideAnimationBboxOffset = new Point(8, 3);

	private static readonly Vector2 VineSwipeAnimationDrawOrigin = new Vector2(24f, 8f);

	private static readonly Vector2 VineIdleAnimationDrawOrigin = new Vector2(4f, 0f);

	private static readonly Vector2 VineHideAnimationDrawOrigin = new Vector2(8f, 3f);

	private static readonly Color FadedColor = new Color(0.675f, 0.7f, 0.65f);

	private readonly Point _basePosition;

	private readonly Appendage _ceilingAppendage;

	private readonly Appendage _leftVineAppendage;

	private readonly Appendage _rightVineAppendage;

	private readonly Appendage _bottomVineAppendage;

	private readonly Appendage _endVineAppendage;

	private readonly CavesSporeVineDamageArea _vineDamageArea;

	private bool _areVinesIntact;

	private ECavesVineType _lastAttackingVineType;

	private ECavesVineType _attackingVineType;

	private int _vineHP;

	private int _lastVineAttackingFrame;

	private float _sporeOscillationDelta;

	private float _momentum;

	private float _swayMultiplier = 1f;

	private Appendage _attackingVine;

	public CavesSporeVine(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		Position = inPosition.Add(8, 0);
		_basePosition = Position;
		_currentAI = EAIStrategy.None;
		_agility = 0.25f;
		_bboxOffset = new Point(2, 2);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		DrawOrigin = new Vector2(10f, 10f);
		_isAffectedByGravity = false;
		_isFlying = true;
		_doesAggroOnTakingDamage = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		_vineHP = (int)((float)base.MaxHP * 0.5f);
		_areVinesIntact = true;
		base.AggroBboxDimensions = new Point(200, 160);
		base.DeaggroBboxDimensions = new Point(350, 256);
		_endVineAppendage = new Appendage(this, new Point(16, 16), Point.Zero, _level, _sprite);
		_endVineAppendage.AnchorObject = _endVineAppendage;
		_ceilingAppendage = new Appendage(_endVineAppendage, new Point(64, 8), new Point(0, 0), _level, _sprite);
		_leftVineAppendage = new Appendage(this, new Point(2, 2), Point.Zero, _level, _sprite);
		_rightVineAppendage = new Appendage(this, new Point(2, 2), Point.Zero, _level, _sprite);
		_bottomVineAppendage = new Appendage(this, new Point(2, 2), Point.Zero, _level, _sprite);
		Position = new Point(Position.X, Position.Y + 97);
		InitializeAppendages();
		ChangeAnimation(1);
		_vineDamageArea = new CavesSporeVineDamageArea(_level, Position, base.DefaultTeam, this, base.Damage);
	}

	private void InitializeAppendages()
	{
		_ceilingAppendage.Position = _basePosition;
		_ceilingAppendage.ChangeAnimation(0);
		_endVineAppendage.ChangeAnimation(-1);
		IdleVine(_leftVineAppendage);
		IdleVine(_rightVineAppendage);
		IdleVine(_bottomVineAppendage);
		_leftVineAppendage.Rotation = (float)Math.PI / 2f;
		_rightVineAppendage.Rotation = -(float)Math.PI / 2f;
		_ceilingAppendage.FollowType = EAppendageFollowType.None;
		_endVineAppendage.FollowType = EAppendageFollowType.None;
		_leftVineAppendage.FollowType = EAppendageFollowType.AnchorLocked;
		_rightVineAppendage.FollowType = EAppendageFollowType.AnchorLocked;
		_bottomVineAppendage.FollowType = EAppendageFollowType.AnchorLocked;
		_leftVineAppendage.DrawPriority = 1;
		_rightVineAppendage.DrawPriority = 1;
		_bottomVineAppendage.DrawPriority = 1;
		_leftVineAppendage.AnchorOffset = new Point(-7, -5);
		_rightVineAppendage.AnchorOffset = new Point(9, -5);
		_bottomVineAppendage.AnchorOffset = new Point(1, 3);
		_ceilingAppendage.StartPointOffset = new Point(0, -6);
		_ceilingAppendage.EndPointOffset = new Point(0, -16);
		_ceilingAppendage.AddJointedLinks(3, 2, new Point(4, 30), new Point(6, -30), Point.Zero, new Vector2(8f, 0f), doHingesHaveAngularLimits: false);
		_appendages.Add(_endVineAppendage);
		_endVineAppendage.AddAppendage(_ceilingAppendage);
		_appendages.Add(_bottomVineAppendage);
		_appendages.Add(_leftVineAppendage);
		_appendages.Add(_rightVineAppendage);
	}

	public override void InitializeMob()
	{
		Update(0f);
		UpdateAppendages(0f);
		base.InitializeMob();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateMomentum(delta);
			UpdateSpore(delta);
			if (!_isRunningDeathScript)
			{
				UpdateVines();
			}
		}
		base.Update(delta);
	}

	private void UpdateMomentum(float delta)
	{
		_momentum -= delta * 3f;
		if (_momentum < 0f)
		{
			_momentum = 0f;
		}
		if (_attackingVine != null && _areVinesIntact)
		{
			_momentum += delta * 5f;
		}
		if (_momentum > 10f)
		{
			_momentum = 10f;
		}
	}

	private void UpdateSpore(float delta)
	{
		_sporeOscillationDelta += delta + delta * _momentum * 0.2f;
		if (_sporeOscillationDelta >= 10000f)
		{
			_sporeOscillationDelta -= 10000f;
		}
		if (!_areVinesIntact && _swayMultiplier > 0f)
		{
			_swayMultiplier -= delta * 0.1f;
			if (_swayMultiplier < 0f)
			{
				_swayMultiplier = 0f;
			}
		}
		float num = _swayMultiplier * 8f;
		int num2 = (int)(Math.Cos(_sporeOscillationDelta * 1.5f) * (double)(num + _momentum * 2f));
		int num3 = (int)(Math.Sin(_sporeOscillationDelta * 1.5f) * (double)(num * 0.15f + _momentum * 0.5f));
		if (num3 < 0)
		{
			num3 = -num3;
		}
		_endVineAppendage.Position = new Point(_basePosition.X + num2, _basePosition.Y + 97 + num3);
		if (_areVinesIntact)
		{
			Position = _endVineAppendage.Position;
		}
	}

	private void UpdateVines()
	{
		if (_isAggroed)
		{
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
			if ((_areVinesIntact && nearestProtagonistPosition.Y > Position.Y + 16) || (!_areVinesIntact && nearestProtagonistPosition.Y < Position.Y - 16))
			{
				_attackingVineType = ECavesVineType.Bottom;
			}
			else if (nearestProtagonistPosition.X > Position.X)
			{
				_attackingVineType = ECavesVineType.Right;
			}
			else
			{
				_attackingVineType = ECavesVineType.Left;
			}
		}
		else
		{
			_attackingVineType = ECavesVineType.None;
		}
		if (_lastAttackingVineType != _attackingVineType)
		{
			_lastVineAttackingFrame = -1;
			switch (_attackingVineType)
			{
			case ECavesVineType.Left:
				DoVineSwipe(_leftVineAppendage);
				DoVineHide(_rightVineAppendage);
				DoVineHide(_bottomVineAppendage);
				break;
			case ECavesVineType.Right:
				DoVineSwipe(_rightVineAppendage);
				DoVineHide(_leftVineAppendage);
				DoVineHide(_bottomVineAppendage);
				break;
			case ECavesVineType.Bottom:
				DoVineSwipe(_bottomVineAppendage);
				DoVineHide(_leftVineAppendage);
				DoVineHide(_rightVineAppendage);
				break;
			case ECavesVineType.None:
				_attackingVine = null;
				switch (_lastAttackingVineType)
				{
				case ECavesVineType.Left:
					EndVineSwipe(_leftVineAppendage);
					IdleVine(_rightVineAppendage);
					IdleVine(_bottomVineAppendage);
					break;
				case ECavesVineType.Right:
					EndVineSwipe(_rightVineAppendage);
					IdleVine(_leftVineAppendage);
					IdleVine(_bottomVineAppendage);
					break;
				case ECavesVineType.Bottom:
					EndVineSwipe(_bottomVineAppendage);
					IdleVine(_leftVineAppendage);
					IdleVine(_rightVineAppendage);
					break;
				}
				break;
			}
		}
		if (_attackingVineType != 0 && _attackingVine != null)
		{
			int num = _attackingVine.AnimationIndex + _attackingVine.AnimationStart;
			if ((num == 22 && _lastVineAttackingFrame != 22) || (num == 27 && _lastVineAttackingFrame != 27))
			{
				PlayCue(ESFX.EnemySporeVineWhiff, _attackingVine.Position);
			}
			_lastVineAttackingFrame = num;
		}
		_lastAttackingVineType = _attackingVineType;
	}

	private void DoVineSwipe(Appendage vine)
	{
		_attackingVine = vine;
		vine.DrawOrigin = VineSwipeAnimationDrawOrigin;
		vine.BboxOffset = VineSwipeAnimationBboxOffset;
		vine.ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 18,
				Length = 2,
				Speed = 0.05f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 20,
				Length = 10,
				Speed = 0.05f,
				Type = EAnimationType.Cycle
			}
		});
		bool flag = _vineDamageArea.Life <= 0f;
		_vineDamageArea.Refresh(_attackingVineType, _areVinesIntact, vine);
		if (flag)
		{
			_level.AddProjectile(_vineDamageArea);
		}
	}

	private static void DoVineHide(Appendage vine)
	{
		vine.DrawOrigin = VineHideAnimationDrawOrigin;
		vine.BboxOffset = VineHideAnimationBboxOffset;
		vine.ChangeAnimation(8, 10, 0.05f, EAnimationType.Cycle);
	}

	private void EndVineSwipe(Appendage vine)
	{
		IdleVine(vine);
		_vineDamageArea.Sleep();
	}

	private static void IdleVine(Appendage vine)
	{
		vine.BboxOffset = VineIdleAnimationBboxOffset;
		vine.DrawOrigin = VineIdleAnimationDrawOrigin;
		vine.ChangeAnimation(3, 5, 0.15f, EAnimationType.Cycle);
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		if (flag && base.HP > 0 && _areVinesIntact)
		{
			_momentum += 1f;
			if (!Bbox.Intersects(sourceRectangle))
			{
				List<Rectangle> list = FindIntersectingBoundingBoxes(sourceRectangle, 1);
				if (list != null && list.Count > 0)
				{
					Rectangle rectangle = list.First();
					if (rectangle != _leftVineAppendage.Bbox && rectangle != _rightVineAppendage.Bbox && rectangle != _bottomVineAppendage.Bbox && rectangle != _ceilingAppendage.Bbox)
					{
						_vineHP -= damage;
						if (_vineHP <= 0)
						{
							DestroyVines();
						}
					}
				}
			}
		}
		return flag;
	}

	private void DestroyVines()
	{
		_areVinesIntact = false;
		_endVineAppendage.DoesCollideWithAnything = false;
		_ceilingAppendage.DrawColor = FadedColor;
		_ceilingAppendage.DoesInheritDrawColor = false;
		_ceilingAppendage.DoesCollideWithAnything = false;
		base.DoesCollideWithTiles = true;
		_isAffectedByGravity = true;
		_isFlying = false;
		_bottomVineAppendage.IsFlippedVertically = true;
		_bottomVineAppendage.AnchorOffset = new Point(1, 20);
		UpdateAppendages(0f);
		PlayCue(ESFX.EnemySporeVineBreak, _basePosition);
	}

	protected override void DoLandingAction()
	{
		PlayCue(ESFX.EnemySporeVineLand, Position);
		ChangeAnimation(30);
		base.DoLandingAction();
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			if (_areVinesIntact)
			{
				DestroyVines();
			}
			_level.AddAnimation(EBattleAnimationType.WetSplashLarge, Bbox.Center);
			_vineDamageArea.Kill();
			base.IsSolidWhenFrozen = false;
			_leftVineAppendage.ChangeAnimation(-1);
			_rightVineAppendage.ChangeAnimation(-1);
			_bottomVineAppendage.ChangeAnimation(-1);
			ChangeAnimation(-1);
			DropLoot();
			_deathScriptTimer += delta;
		}
		UpdateAnimation(delta);
		UpdateAppendages(delta);
	}
}
