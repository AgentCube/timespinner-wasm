using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses;

internal sealed class GodBirdFloorGoopEvent : GameEvent
{
	private const float TimeBeforeFading = 8f;

	private const float TimeToFade = 0.2f;

	private const float GoopGravity = 120f;

	private const float TimeForFootstepCooldown = 0.25f;

	private const float GoopVelocityMultiplier = 0.75f;

	private readonly Appendage _splatAppendage;

	private bool _isFading;

	private float _lifeTimer;

	private float _fadeTimer;

	private float _footstepCueCooldownTimer;

	private Vector2 _currentVector = Vector2.Zero;

	internal bool IsActive { get; private set; }

	public GodBirdFloorGoopEvent(Level inLevel, Point inPosition, ObjectTileSpecification objectSpec, SpriteSheet sprite)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_sprite = sprite;
		_bbox = new Rectangle(0, 0, 64, 17);
		_bboxOffset = new Point(0, -1);
		_doesUseAppendageCollision = false;
		_isSolid = false;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = true;
		IsFacingLeft = true;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = true;
		_defaultTeam = ETeamSide.Neutral;
		base.DrawPlane = EDrawPlane.Front;
		_isAffectedByTime = true;
		base.CanBeUsedWhenFrozen = true;
		base.IsLostWhenNotTouching = true;
		base.IsLostWhenNotGrounded = true;
		_currentVector = new Vector2(0f, 120f);
		ChangeAnimation(35);
		_doesDrawBaseSprite = false;
		_splatAppendage = new Appendage(this, new Point(32, 18), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked
		};
		base.Appendages.Add(_splatAppendage);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = false;
		if (IsActive)
		{
			who.AddMovingPlatform(this);
			_currentVector = new Vector2((0f - who.Velocity.X) * 0.75f, 0f);
			if (_footstepCueCooldownTimer <= 0f && !_isFading && Math.Abs(who.Velocity.X) > 1f)
			{
				PlayCue(ESFX.EnemyCheveuxTowerVomitWalk, who.Position);
				_footstepCueCooldownTimer = 0.25f;
			}
			result = base.TriggerEvent(who, depth);
		}
		return result;
	}

	public override void Update(float delta)
	{
		int animationIndex = _splatAppendage.AnimationIndex;
		if (!_doesDrawBaseSprite && animationIndex == 3 && _splatAppendage.IsAnimationDone)
		{
			_doesDrawBaseSprite = true;
			_splatAppendage.ChangeAnimation(-1);
		}
		if (!base.IsFrozen)
		{
			_lifeTimer += delta;
			if (_lifeTimer > 8f)
			{
				_isFading = true;
				_fadeTimer += delta;
				if (_fadeTimer < 0.2f)
				{
					base.DrawColor = Color.White * (1f - _fadeTimer / 0.2f);
				}
				else
				{
					_level.RequestRemoveObject(this);
					IsActive = false;
				}
			}
			base.Update(delta);
		}
		if (!_splatAppendage.IsAnimationDone && (_splatAppendage.AnimationIndex != animationIndex || _splatAppendage.AnimationIndex == 0))
		{
			switch (_splatAppendage.AnimationIndex)
			{
			case 0:
				_splatAppendage.AnchorOffset = new Point(-10, -14);
				break;
			case 1:
				_splatAppendage.AnchorOffset = new Point(-10, -13);
				break;
			case 2:
				_splatAppendage.AnchorOffset = new Point(-16, -9);
				break;
			case 3:
				_splatAppendage.AnchorOffset = new Point(-16, -3);
				break;
			}
			UpdateAppendages(0f);
		}
		if (_footstepCueCooldownTimer > 0f)
		{
			_footstepCueCooldownTimer -= delta;
		}
		base.AmountMovedLastStep = new Vector2(_currentVector.X * delta, _currentVector.Y * delta);
	}

	internal void Reset(Point contactPoint)
	{
		if (!IsActive)
		{
			base.ID = -1;
		}
		IsActive = true;
		_isFading = false;
		_fadeTimer = 0f;
		_lifeTimer = 0f;
		_footstepCueCooldownTimer = 0f;
		base.DrawColor = Color.White;
		_doesDrawBaseSprite = false;
		Position = contactPoint.Add(0, 16);
		SnapBboxToPosition();
		Update(0f);
		_splatAppendage.ChangeAnimation(-1);
		_splatAppendage.ChangeAnimation(31, 4, 0.067f, EAnimationType.Once);
	}
}
