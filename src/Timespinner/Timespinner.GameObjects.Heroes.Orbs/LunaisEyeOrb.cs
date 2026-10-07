using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisEyeOrb : LunaisOrb
{
	private const int OrbTrailLength = 2;

	private const float ThrowTimeToAttack = 0.5f;

	private const float ThrowTimeToReturn = 0.5f;

	private const float ThrowRadius = 128f;

	private const int TransformAnimationLength = 4;

	private const int LargeAppendageFrameStart = 9;

	private const int SmallAppendageFrameStart = 14;

	private const int FleshyAppendageFrameStart = 19;

	private const float TransformationAnimationSpeed = 0.07f;

	private const float TimeBeforeHiding = 0.64f;

	private const float RotationSpeed = 25f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private static readonly Color WhiteTrailColor = new Color(0.95f, 0.95f, 1f, 0.8f);

	private static readonly Color ThrowTrailColor = new Color(0.8f, 0.4f, 0.4f, 0.3f);

	private readonly Appendage _largeTopAppendage;

	private readonly Appendage _largeBottomAppendage;

	private readonly Appendage _smallRightAppendage;

	private readonly Appendage _smallLeftAppendage;

	private readonly Appendage _fleshyTopAppendage;

	private readonly Appendage _fleshyBottomAppendage;

	private float _effectiveRadius;

	private Point _throwStartPoint;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Eye;

	public LunaisEyeOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 2;
		_doesDrawBrushTrail = false;
		ClearTrailHistory();
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(0, 9, 0.1f, EAnimationType.PingPong);
		Update(0f);
		_largeTopAppendage = new Appendage(this, new Point(1, 1), new Point(9, 26), _level, _sprite)
		{
			DrawOrigin = new Vector2(9.5f, 25f),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_largeBottomAppendage = new Appendage(this, new Point(1, 1), new Point(9, 26), _level, _sprite)
		{
			DrawOrigin = new Vector2(9.5f, 25f),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_smallRightAppendage = new Appendage(this, new Point(1, 1), new Point(8, 17), _level, _sprite)
		{
			DrawOrigin = new Vector2(7.5f, 17f),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_smallLeftAppendage = new Appendage(this, new Point(1, 1), new Point(8, 17), _level, _sprite)
		{
			DrawOrigin = new Vector2(7.5f, 17f),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_fleshyTopAppendage = new Appendage(this, new Point(1, 1), new Point(6, 1), _level, _sprite)
		{
			DrawOrigin = new Vector2(6.5f, 1f),
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(-1, -2)
		};
		_fleshyBottomAppendage = new Appendage(this, new Point(1, 1), new Point(6, 1), _level, _sprite)
		{
			DrawOrigin = new Vector2(6.5f, 1f),
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(0, 4)
		};
		_appendages.Add(_largeTopAppendage);
		_appendages.Add(_largeBottomAppendage);
		_appendages.Add(_smallRightAppendage);
		_appendages.Add(_smallLeftAppendage);
		_appendages.Add(_fleshyTopAppendage);
		_appendages.Add(_fleshyBottomAppendage);
		_doesDrawAppendages = false;
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_trailColor = ThrowTrailColor;
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X - 3, Bbox.Center.Y), _level)
		{
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = base.IsOrbFacingLeft,
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 69,
			AnimationLength = 4
		});
		_level.AddProjectile(new EyeOrbMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.OrbDamage, this));
		_doesDrawAppendages = true;
		_largeTopAppendage.ChangeAnimation(9, 4, 0.07f, EAnimationType.Once);
		_largeBottomAppendage.ChangeAnimation(9, 4, 0.07f, EAnimationType.Once);
		_smallRightAppendage.ChangeAnimation(14, 4, 0.07f, EAnimationType.Once);
		_smallLeftAppendage.ChangeAnimation(14, 4, 0.07f, EAnimationType.Once);
		_fleshyTopAppendage.ChangeAnimation(19, 4, 0.07f, EAnimationType.Once);
		_fleshyBottomAppendage.ChangeAnimation(19, 4, 0.07f, EAnimationType.Once);
		_largeTopAppendage.Rotation = 0f;
		_largeBottomAppendage.Rotation = (float)Math.PI;
		_smallRightAppendage.Rotation = (float)Math.PI / 2f;
		_smallLeftAppendage.Rotation = 4.712389f;
		_fleshyBottomAppendage.Rotation = 0f;
		_fleshyTopAppendage.Rotation = (float)Math.PI;
		_throwStartPoint = Position;
		_effectiveRadius = 128f + (float)((base.IsThrowingLeft ? 1 : (-1)) * (Position.X - base.CurrentTarget.X));
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		UpdateUnhookedThrowAttack(delta, 0.5f, 0.5f, _throwStartPoint, UpdateThrow);
	}

	private Vector2 UpdateThrow(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		if (_currentThrowTime > 1f)
		{
			return EndAttack();
		}
		if (_currentThrowTime >= 0.5f && currentThrowTime < 0.5f)
		{
			base.IsAtAttackApex = true;
		}
		if (_currentThrowTime >= 0.64f && currentThrowTime < 0.64f)
		{
			_largeTopAppendage.ChangeAnimation(new AnimationSpec
			{
				Start = 9,
				Length = 4,
				Speed = 0.07f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
			_largeBottomAppendage.ChangeAnimation(new AnimationSpec
			{
				Start = 9,
				Length = 4,
				Speed = 0.07f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
			_smallRightAppendage.ChangeAnimation(new AnimationSpec
			{
				Start = 14,
				Length = 4,
				Speed = 0.07f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
			_smallLeftAppendage.ChangeAnimation(new AnimationSpec
			{
				Start = 14,
				Length = 4,
				Speed = 0.07f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
			_fleshyTopAppendage.ChangeAnimation(new AnimationSpec
			{
				Start = 19,
				Length = 4,
				Speed = 0.07f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
			_fleshyBottomAppendage.ChangeAnimation(new AnimationSpec
			{
				Start = 19,
				Length = 4,
				Speed = 0.07f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
		}
		float num = delta * 25f;
		foreach (Appendage appendage in _appendages)
		{
			if (appendage.BboxOffset.X != 6)
			{
				appendage.Rotation += num;
			}
		}
		float num2 = ((_currentThrowTime <= 0.5f) ? (_currentThrowTime / 1f) : (0.5f + (_currentThrowTime - 0.5f) / 1f));
		_trailColor = ((_currentThrowTime <= 0.6f) ? WhiteTrailColor : ThrowTrailColor);
		float num3 = _effectiveRadius * (float)Math.Sin(num2 * ((float)Math.PI / 2f));
		_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num3;
		return new Vector2((int)Math.Round((float)_throwStartPoint.X + _orbXShift), _throwStartPoint.Y);
	}

	private Vector2 EndAttack()
	{
		base.State = EOrbState.Idle;
		_trailColor = base.IdleTrailColor;
		Vector2 result = _baseOrbitPosition.ToVector2();
		base.Rotation = 0f;
		_doesDrawAppendages = false;
		return result;
	}

	public override void ChangeRoom()
	{
		if (base.State == EOrbState.Melee)
		{
			EndAttack();
		}
		base.ChangeRoom();
	}

	public override void Kill()
	{
		ClearTrailHistory();
		foreach (Appendage appendage in base.Appendages)
		{
			appendage.ChangeAnimation(-1);
		}
		base.Kill();
	}
}
