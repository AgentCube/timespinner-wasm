using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ShapeshifterArrow : Projectile
{
	private const int WidthHeight = 16;

	private const int HalfWidthHeight = 8;

	private const int WallLeftX = 32;

	private const int WallRightX = 368;

	private const int FloorY = 224;

	private const int CeilingY = 32;

	private const int BaseHeightOffset = 80;

	private const int OffsetXIndexMultiplier = 48;

	private const int OffsetYIndexMultiplier = 48;

	private const float FireTimeOffsetMultiplier = 0.1f;

	private const float TimeToAim = 0.33f;

	private const float VelocityMultiplier = 700f;

	private readonly bool _isOnLeftSideOfRoom;

	private readonly int _arrowDamage;

	private readonly int _arrowIndex;

	private readonly int _finalXOffset;

	private readonly int _finalYOffset;

	private readonly float _timeToGetIntoPosition;

	private readonly float _timeBeforeFiring;

	private readonly Point _startingPoint;

	private readonly Protagonist _targetProtagonist;

	private bool _isLodgedInWall;

	private bool _hasFired;

	private float _arrowTimer;

	private float _arrowAngle;

	public ShapeshifterArrow(Level inLevel, Point inPosition, SpriteSheet sprite, Protagonist targetProtagonist, bool isOnLeftSideOfRoom, int index, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, ETeamSide.Enemies, -1)
	{
		_targetProtagonist = targetProtagonist;
		_arrowIndex = index;
		_sprite = sprite;
		_startingPoint = inPosition;
		_isOnLeftSideOfRoom = isOnLeftSideOfRoom;
		_arrowDamage = (int)Math.Ceiling((float)baseDamage * 1.1f);
		_bbox = new Rectangle(inPosition.X - 8, inPosition.Y - 8, 16, 16);
		_bboxOffset = Point.Zero;
		_power = 0;
		_force = 0;
		_life = 3f;
		base.CanDamageThings = false;
		_doesRotateBasedOnVelocity = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesCollideWithWalls = false;
		_doesDieOnTiles = false;
		base.DoesCollideWithTiles = false;
		base.DoesDieToEnemyProjectiles = false;
		_doesProjectileChangeFacingBasedOnVelocity = false;
		ChangeAnimation(68, 3, 0.1f, EAnimationType.Cycle);
		_timeToGetIntoPosition = 0.75f + 0.1f * (float)_arrowIndex;
		_timeBeforeFiring = _timeToGetIntoPosition + 0.33f;
		_finalXOffset = 48 * _arrowIndex;
		_finalYOffset = 80 + 48 * _arrowIndex;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			float arrowTimer = _arrowTimer;
			_arrowTimer += delta;
			if (_arrowTimer <= _timeToGetIntoPosition)
			{
				float num = _arrowTimer / _timeToGetIntoPosition;
				int num2 = (int)((1.0 - Math.Cos(num * ((float)Math.PI / 2f))) * (double)_finalXOffset) * (_isOnLeftSideOfRoom ? 1 : (-1));
				int num3 = (int)(Math.Sin(num * ((float)Math.PI / 2f)) * (double)_finalYOffset);
				Position = new Point(_startingPoint.X + num2, _startingPoint.Y - num3);
			}
			else if (_arrowTimer < _timeBeforeFiring)
			{
				if (arrowTimer <= _timeToGetIntoPosition)
				{
					PlayCue(ESFX.BossShapeshifterSpearForm);
					ChangeAnimation(62, 6, 0.07f, EAnimationType.Once);
					BboxOffset = new Point(15, 3);
					Bbox = new Rectangle(0, 0, 10, 10);
					DrawOrigin = new Vector2(20f, 8f);
					SnapBboxToPosition();
					if (_targetProtagonist != null)
					{
						Point point = new Point(Position.X - _targetProtagonist.Position.X, Position.Y - _targetProtagonist.Bbox.Top);
						_arrowAngle = (float)Math.Atan2(point.Y, point.X);
						base.Rotation = _arrowAngle;
					}
				}
			}
			else if (!_hasFired)
			{
				_hasFired = true;
				base.CanDamageThings = true;
				base.Power = _arrowDamage;
				base.Velocity = new Vector2((0f - (float)Math.Cos(_arrowAngle)) * 700f, (0f - (float)Math.Sin(_arrowAngle)) * 700f);
				PlayCue(ESFX.BossShapeshifterSpearThrow);
			}
		}
		base.Update(delta);
		if (!base.IsFrozen && _hasFired && !_isLodgedInWall && (Bbox.Left <= 32 || Bbox.Right >= 368 || Bbox.Bottom >= 224 || Bbox.Top <= 32))
		{
			KillOnWall(Position);
		}
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		_level.PlayCue(ESFX.BossShapeshifterSpearImpact, target.Position);
		base.AddImpactAnimation(target, collidingRectangle);
	}

	private void KillOnWall(Point contactPoint)
	{
		_isLodgedInWall = true;
		_power = 0;
		_velocity = Vector2.Zero;
		_canDamageThings = false;
		PlayCue(ESFX.BossShapeshifterSpearImpact, contactPoint);
		_life = Math.Min(_life, 1f);
	}

	public override void Kill(bool useAnimation)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, _bbox.Center, _teamSide);
		Kill();
	}
}
