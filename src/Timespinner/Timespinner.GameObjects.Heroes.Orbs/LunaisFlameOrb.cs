using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisFlameOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const float ThrowTimeToAttack = 0.3f;

	private const float ThrowTimeToReturn = 0.4f;

	private const float TimeBeforeAddingFireballs = 0.01f;

	private const float FireballLifeTime = 0.29000002f;

	private const float ThrowRadius = 55f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private static readonly Color WhiteTrailColor = new Color(0.8f, 0.65f, 0.55f, 0.8f);

	private static readonly Color ThrowTrailColor = new Color(1f, 0.8f, 0.75f, 0.5f);

	private readonly FlameOrbMeleeDamageArea _topDamageArea;

	private readonly FlameOrbMeleeDamageArea _bottomDamageArea;

	private BattleAnimation _startAnimation;

	private BattleAnimation _endAnimation;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Flame;

	public LunaisFlameOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(15, 10, 0.05f, EAnimationType.Cycle);
		_topDamageArea = new FlameOrbMeleeDamageArea(_level, Position, this, 0.29000002f, isTopFireball: true, base.OrbDamage, this, _sprite);
		_bottomDamageArea = new FlameOrbMeleeDamageArea(_level, Position, this, 0.29000002f, isTopFireball: false, base.OrbDamage, this, _sprite);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_trailColor = ThrowTrailColor;
		_level.PlayCue(ESFX.LunaisOrbFire, Position);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		UpdateThrowAttack(delta, 0.3f, 0.4f, UpdateThrow);
	}

	private Vector2 UpdateThrow(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		if (currentThrowTime <= 0f)
		{
			if (_startAnimation == null)
			{
				_startAnimation = new BattleAnimation(_sprite, Bbox.Center, _level)
				{
					TeamSide = ETeamSide.Heroes,
					AnimationStart = 25,
					AnimationLength = 5,
					IsFacingLeft = base.IsThrowingLeft
				};
			}
			else
			{
				_startAnimation.Reset(Bbox.Center, base.IsThrowingLeft);
			}
			_battleAnimations.Add(_startAnimation);
			_doesDrawBaseSprite = false;
			_doesDrawTrail = false;
		}
		if (currentThrowTime < 0.01f && _currentThrowTime >= 0.01f)
		{
			_topDamageArea.ResetPosition(Bbox.Center, base.IsThrowingLeft);
			_bottomDamageArea.ResetPosition(Bbox.Center, base.IsThrowingLeft);
			_level.AddProjectile(_topDamageArea);
			_level.AddProjectile(_bottomDamageArea);
		}
		if (_currentThrowTime > 0.70000005f)
		{
			base.State = EOrbState.Idle;
			_trailColor = base.IdleTrailColor;
			return _baseOrbitPosition.ToVector2();
		}
		if (_currentThrowTime >= 0.3f && currentThrowTime < 0.3f)
		{
			base.IsAtAttackApex = true;
			if (_endAnimation == null)
			{
				_endAnimation = new BattleAnimation(_sprite, Bbox.Center, _level)
				{
					TeamSide = ETeamSide.Heroes,
					AnimationStart = 30,
					AnimationLength = 5,
					IsFacingLeft = !base.IsThrowingLeft
				};
			}
			else
			{
				_endAnimation.Reset(Bbox.Center, !base.IsThrowingLeft);
			}
			_battleAnimations.Add(_endAnimation);
			_doesDrawBaseSprite = true;
			_doesDrawTrail = true;
			ClearTrailHistory();
			_level.RequestRemoveObject(_topDamageArea);
			_level.RequestRemoveObject(_bottomDamageArea);
		}
		float num = ((_currentThrowTime <= 0.3f) ? (_currentThrowTime / 0.6f) : (0.5f + (_currentThrowTime - 0.3f) / 0.8f));
		_trailColor = ((_currentThrowTime <= 0.4f) ? WhiteTrailColor : ThrowTrailColor);
		float num2 = 55f * (float)Math.Sin((double)num * Math.PI);
		_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num2;
		return new Vector2((int)Math.Round((float)base.CurrentTarget.X + _orbXShift), base.CurrentTarget.Y);
	}

	internal override void UpdateDamage(int damage)
	{
		base.UpdateDamage(damage);
		if (_topDamageArea != null)
		{
			_topDamageArea.UpdateDamage(damage);
		}
		if (_bottomDamageArea != null)
		{
			_bottomDamageArea.UpdateDamage(damage);
		}
	}

	public override void DisposeOrb()
	{
		if (_topDamageArea != null)
		{
			_topDamageArea.SilentKill();
		}
		if (_bottomDamageArea != null)
		{
			_bottomDamageArea.SilentKill();
		}
		base.DisposeOrb();
	}
}
