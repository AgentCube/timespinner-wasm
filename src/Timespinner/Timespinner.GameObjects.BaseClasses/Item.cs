using System;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.BaseClasses;

public class Item : Animate
{
	private const float MagneticRadius = 200000f;

	protected bool _doesFloatInPlace;

	protected bool _doesFallToGround;

	protected bool _doesItemBounceOnGround;

	protected bool _doesFollowPlayer;

	protected float _itemAmount;

	private float _currentFollowTime = 3.14f;

	public EItemType ItemType { get; protected set; }

	public Item(Level inLevel, Point inPosition, EItemType type, float amount, int inID)
		: base(inPosition, inLevel, inID)
	{
		ItemType = type;
		_itemAmount = amount;
		_sprite = _level.GCM.SpItems;
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 16);
		_bboxOffset = new Point(0, 0);
		base.BaseType = EGameObjectBaseType.Item;
		_isAffectedByGravity = false;
		_isAffectedByTime = false;
		_maxFallSpeed = 100f;
		_gravityAcceleration = 500f;
		_doesUse16X16TileCollisionBbox = true;
		base.DoesDrawWhenOutsideOfObjectVisibleArea = false;
		_doesItemBounceOnGround = true;
	}

	private void InitializeBasedOnType()
	{
		DrawOrigin = new Vector2((float)_bbox.Width / 2f, (float)_bbox.Height / 2f);
		SnapBboxToPosition();
		switch (ItemType)
		{
		case EItemType.HP:
			_itemAmount = (float)Math.Ceiling(_itemAmount / 3f);
			_doesFollowPlayer = true;
			_doesDrawTrail = true;
			_trailFadeRate = 1.25f;
			_trailLength = 5;
			_trailShrinkRate = 0.15f;
			ChangeAnimation(0, 8, 0.08f, EAnimationType.Cycle);
			_level.AddAnimation(EBattleAnimationType.HPCreate, Bbox.Center, ETeamSide.Heroes);
			break;
		case EItemType.Sand:
			_doesFollowPlayer = true;
			_doesDrawTrail = true;
			_doesFallToGround = true;
			_doesFloatInPlace = true;
			_trailFadeRate = 1.25f;
			_trailLength = 5;
			_trailShrinkRate = 0.15f;
			ChangeAnimation(9, 10, 0.08f, EAnimationType.Cycle);
			_level.AddAnimation(EBattleAnimationType.MPCreate, Bbox.Center, ETeamSide.Heroes);
			_velocity = new Vector2(_level.NextRandomInt(-75, 75), -100f);
			break;
		case EItemType.Aura:
			break;
		}
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			if (_doesFollowPlayer)
			{
				if (_level.Heroes.Count != 0)
				{
					Protagonist mainHero = _level.MainHero;
					if (mainHero.IsCurrentlyMagnetizing)
					{
						Point currentMagnetCenter = mainHero.CurrentMagnetCenter;
						currentMagnetCenter.Y += 8;
						if (new Vector2(currentMagnetCenter.X - _position.X, currentMagnetCenter.Y - _position.Y).LengthSquared() < 200000f)
						{
							GoToPoint(currentMagnetCenter, delta);
						}
						else
						{
							IdleInPlace(delta);
						}
					}
					else if (_doesFloatInPlace)
					{
						IdleInPlace(delta);
					}
				}
			}
			else if (_doesFloatInPlace)
			{
				IdleInPlace(delta);
			}
			base.Update(delta);
			if (_doesFallToGround)
			{
				_isAffectedByGravity = true;
				_doesBounceOnGround = _doesItemBounceOnGround;
				if (DetectTileCollisions())
				{
					if (_doesFloatInPlace)
					{
						_currentFollowTime = 0f;
						_isAffectedByGravity = false;
						_doesFallToGround = false;
						_velocity.X = 0f;
					}
					else if (_isGrounded && _wasGrounded)
					{
						_animationType = EAnimationType.Once;
					}
				}
			}
		}
		UpdateIsWithinObjectVisibleArea();
	}

	public virtual void GetItem(Protagonist who)
	{
		switch (ItemType)
		{
		case EItemType.HP:
			if (who.IsPrimaryPlayer)
			{
				who.ManageHeal(_itemAmount, shouldShowAnimation: true);
				_level.PlayCue(ESFX.ItemGetRestore, Bbox.Center);
				_level.AddAnimation(EBattleAnimationType.HPSparkles, Bbox.Center, who.DefaultTeam);
				Kill();
			}
			break;
		case EItemType.Sand:
		{
			Protagonist protagonist = (who.IsPrimaryPlayer ? who : _level.MainHero);
			if (protagonist != null)
			{
				protagonist.ManageManaRestore(_itemAmount);
				_level.PlayCue(ESFX.ItemGetRestore, Bbox.Center);
				_level.AddAnimation(EBattleAnimationType.MPSparkles, Bbox.Center, protagonist.DefaultTeam);
				Kill();
			}
			break;
		}
		case EItemType.Aura:
			break;
		}
	}

	private void GoToPoint(Point target, float delta)
	{
		_currentFollowTime += delta;
		if (_currentFollowTime > 314f)
		{
			_currentFollowTime -= 314f;
		}
		float scaleFactor = (float)Math.Sin(_currentFollowTime * 2f) * 200f;
		Vector2 value = Vector2.Normalize(new Vector2(target.X - _position.X, target.Y - _position.Y));
		_velocity = Vector2.Multiply(value, 300f);
		_velocity = Vector2.Add(_velocity, Vector2.Multiply(new Vector2(0f - value.Y, value.X), scaleFactor));
		if ((ItemType == EItemType.Sand || ItemType == EItemType.HP) && _animationLength != 4)
		{
			_animationLength = ((ItemType == EItemType.Sand) ? 4 : 3);
		}
	}

	private void IdleInPlace(float delta)
	{
		_currentFollowTime += delta;
		if (_currentFollowTime > 314f)
		{
			_currentFollowTime -= 314f;
		}
		float num = delta * 60f;
		float x = (float)Math.Cos(_currentFollowTime * 4.5f) * 3f * num;
		float y = (float)Math.Sin((0f - _currentFollowTime) * 4.5f) * 3f * num;
		if (_doesFloatInPlace)
		{
			x = 0f;
		}
		_velocity = Vector2.Add(_velocity, new Vector2(x, y));
		if (_doesFallToGround && ItemType == EItemType.Sand)
		{
			_animationLength = 4;
		}
		else if (ItemType == EItemType.Sand)
		{
			_animationLength = 10;
		}
	}

	internal virtual void Initialize()
	{
		InitializeBasedOnType();
		Update(0f);
	}

	protected override bool HandleHorizontalCollision(GameObject target, Point tileKey, ETileType tileType, Vector2 depth)
	{
		return false;
	}
}
