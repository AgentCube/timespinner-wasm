using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisGunOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const int BulletEmitOffsetX = 28;

	private const int BulletEmitOffsetY = -3;

	private const float GunAppearAnimationSpeed = 0.033f;

	private const float GunRecoilAnimationSpeed = 0.04f;

	private const float ThrowTimeToAttack = 0.16499999f;

	private const float ThrowTimeToReturn = 0.64f;

	private const float TimeBeforeTurningBackIntoOrb = 0.28f;

	private const float ThrowRadius = 20f;

	private const float TimeToChangeTrailColor = 1f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private static readonly Color IdleTrailColor1 = new Color(0.5f, 0.15f, 0.5f, 0.1f);

	private static readonly Color IdleTrailColor2 = new Color(0.75f, 0.15f, 0.15f, 0.1f);

	private readonly BattleAnimation _boomAnimation;

	private readonly Appendage _gunAppendage;

	private bool _isIncrementingTrailTimer;

	private float _trailChangeTimer;

	private Color _idleTrailColor;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Gun;

	public LunaisGunOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = IdleTrailColor1;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(0, 10, 0.05f, EAnimationType.Cycle);
		_doesUseAppendageCollision = false;
		_doAppendagesInheritDrawColor = false;
		_boomAnimation = new BattleAnimation(_sprite, Position, _level)
		{
			AnimationStart = 20,
			AnimationLength = 3,
			AnimationSpeed = 0.035f,
			DoesFadeOut = false
		};
		_gunAppendage = new Appendage(this, new Point(8, 8), new Point(16, 7), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(0, 2)
		};
		_gunAppendage.ChangeAnimation(-1);
		base.Appendages.Add(_gunAppendage);
		Update(0f);
	}

	public override void Update(float delta, Point targetPoint)
	{
		if (!base.IsFrozen)
		{
			_trailChangeTimer += (_isIncrementingTrailTimer ? delta : (0f - delta));
			if (_trailChangeTimer > 1f)
			{
				_trailChangeTimer = 1f;
				_isIncrementingTrailTimer = false;
			}
			if (_trailChangeTimer < 0f)
			{
				_trailChangeTimer = 0f;
				_isIncrementingTrailTimer = true;
			}
			float amount = _trailChangeTimer / 1f;
			_idleTrailColor = IdleTrailColor1.SineInterpolate(IdleTrailColor2, amount);
			if (base.State == EOrbState.Idle)
			{
				_trailColor = _idleTrailColor;
			}
		}
		base.Update(delta, targetPoint);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X - 3, Bbox.Center.Y), _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 69,
			AnimationLength = 4,
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = base.IsOrbFacingLeft
		});
		ChangeAnimation(-1);
		_gunAppendage.IsFacingLeft = base.IsThrowingLeft;
		_gunAppendage.ChangeAnimation(10, 4, 0.033f, EAnimationType.Once);
		_level.PlayCue(ESFX.LunaisOrbGunMelee, Position);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		UpdateThrowAttack(delta, 0.16499999f, 0.64f, UpdateThrow);
	}

	private Vector2 UpdateThrow(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		if (_currentThrowTime > 0.805f)
		{
			base.State = EOrbState.Idle;
			return _baseOrbitPosition.ToVector2();
		}
		if (_currentThrowTime >= 0.16499999f && currentThrowTime < 0.16499999f)
		{
			base.IsAtAttackApex = true;
			Point point = Position.Add(28 * ((!base.IsThrowingLeft) ? 1 : (-1)), -3);
			Vector2 value = new Vector2((!base.IsThrowingLeft) ? 1 : (-1), 0f);
			value = Vector2.Multiply(value, 10000f);
			_level.AddProjectile(new GunOrbMeleeProjectile(_level, point, value, ETeamSide.Heroes, this, base.OrbDamage, _sprite));
			_gunAppendage.ChangeAnimation(13, 7, 0.04f, EAnimationType.Once);
			_boomAnimation.Reset(point, base.IsThrowingLeft);
			_level.AddAnimation(_boomAnimation);
		}
		float num = ((_currentThrowTime <= 0.16499999f) ? (_currentThrowTime / 0.32999998f) : (0.5f + (_currentThrowTime - 0.16499999f) / 1.28f));
		float num2 = 20f * (float)Math.Sin((double)num * Math.PI);
		_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num2;
		Vector2 result = new Vector2((int)Math.Round((float)base.CurrentTarget.X + _orbXShift), base.CurrentTarget.Y);
		if (_currentThrowTime >= 0.445f)
		{
			if (currentThrowTime < 0.445f)
			{
				ChangeAnimation(0, 10, 0.05f, EAnimationType.Cycle);
				_gunAppendage.ChangeAnimation(-1);
			}
		}
		else
		{
			ClearTrailHistory();
		}
		return result;
	}
}
