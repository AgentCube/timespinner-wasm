using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._14_Gyre;

namespace Timespinner.GameObjects.Enemies;

internal sealed class GyreZelSwordAppendage : Appendage
{
	private const int SwordFrameIndex = 32;

	private const int SuccessiveSwordOffsetY = -16;

	private const int SwordSwipeOffsetY = 8;

	private const int SwordSwipeRadius = 24;

	private const int SwordShotRadius = 400;

	private const int SwordSwipeDamageMinSize = 8;

	private const int SwordSwipeDamageOffset = 8;

	private const int SwordSwipeDamageRadius = 32;

	private const float TimeForSwordSwipe = 0.2f;

	private const float TimeForSwordShot = 0.75f;

	private const float TimeForFadeOut = 0.1f;

	private const float TimeForTotalAttack = 0.95f;

	private const float TimeBeforeFadingOut = 0.85f;

	private const float SwordSwipeStartAngle = (float)Math.PI * 27f / 40f;

	private const float SwordSwipeEndAngle = 0f;

	private const float SwordSwipeTimeOffset = 0.35f;

	private const float SwordSwipeTimeMultiplier = 1.35f;

	private readonly int _index;

	private readonly GyreZelSwordDamageArea _damageArea;

	private bool _isThrowingLeft;

	private float _attackTimer;

	private Point _swordShotStart;

	internal bool IsFinished { get; set; }

	internal bool IsAvailable { get; set; }

	internal Point CurrentTarget { get; set; }

	public GyreZelSwordAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite, int index)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		_index = index;
		ChangeAnimation(32);
		_damageArea = new GyreZelSwordDamageArea(_level, Position, ETeamSide.Enemies, 0, _sprite);
		IsAvailable = true;
		base.DoesDrawTrail = true;
		base.TrailLength = 4;
		base.TrailFadeRate = 4f;
		base.TimeToTurnAround = 0f;
		base.DoesInheritDrawColor = false;
		base.IsFacingLocked = true;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !IsFinished)
		{
			float attackTimer = _attackTimer;
			_attackTimer += delta;
			if (_attackTimer < 0.95f)
			{
				int num = -16 * _index;
				if (_attackTimer < 0.2f)
				{
					float num2 = _attackTimer / 0.2f;
					float start = (_isThrowingLeft ? ((float)Math.PI * 27f / 40f) : ((float)Math.PI * -27f / 40f));
					float rotation = MathEx.SineInterpolate(start, 0f, num2);
					base.Rotation = rotation;
					float num3 = num2 * 1.35f - 0.35f;
					double num4 = Math.Sin(num3 * ((float)Math.PI / 2f));
					double num5 = Math.Cos(num3 * ((float)Math.PI / 2f));
					int num6 = (int)Math.Ceiling(num4 * 24.0 * (double)((!_isThrowingLeft) ? 1 : (-1)));
					int num7 = -(int)Math.Ceiling(num5 * 24.0) + 8 + num;
					Position = new Point(CurrentTarget.X + num6, CurrentTarget.Y + num7);
					int width = MathEx.Max(8, (int)Math.Ceiling(num4 * 32.0));
					int height = MathEx.Max(8, (int)Math.Ceiling(num5 * 32.0));
					Point position = new Point(Position.X + 8 * ((!_isThrowingLeft) ? 1 : (-1)), Position.Y - 8);
					_damageArea.SetBoundingBox(position, width, height);
					if (num2 < 0.5f)
					{
						base.IsGlowing = true;
						_glowColor = new Color(1f, 1f, 1f, 0.5f);
						_glowBase = MathEx.SineInterpolate(12f, 1f, num2 * 2f);
						_scale = MathEx.SineInterpolate(0.1f, 1f, num2 * 2f);
					}
					else
					{
						base.IsGlowing = false;
						base.DrawColor = Color.White;
						_scale = 1f;
					}
				}
				else
				{
					if (attackTimer < 0.2f)
					{
						_swordShotStart = Position;
						base.Rotation = 0f;
						_damageArea.SetBoundingBox(Position, 36, 10);
						base.IsGlowing = false;
						base.DrawColor = Color.White;
						_scale = 1f;
					}
					float num8 = (_attackTimer - 0.2f) / 0.75f;
					int num9 = (int)Math.Ceiling(num8 * 400f) * ((!_isThrowingLeft) ? 1 : (-1));
					Position = new Point(_swordShotStart.X + num9, _swordShotStart.Y);
					Point position2 = new Point(Position.X + 18 * ((!_isThrowingLeft) ? 1 : (-1)), Position.Y);
					_damageArea.Position = position2;
				}
				if (_attackTimer >= 0.85f)
				{
					float num10 = (_attackTimer - 0.85f) / 0.1f;
					base.DrawColor = Color.White * (1f - num10);
				}
			}
			else
			{
				IsFinished = true;
				_damageArea.SilentKill();
			}
		}
		base.Update(delta);
	}

	internal void Reset(Point target, bool isThrowingLeft, int damage)
	{
		IsFinished = false;
		IsAvailable = false;
		_attackTimer = 0f;
		CurrentTarget = target;
		_isThrowingLeft = isThrowingLeft;
		IsFacingLeft = isThrowingLeft;
		DrawOrigin = new Vector2(isThrowingLeft ? 34 : 0, 8f);
		base.DrawColor = Color.White;
		ClearTrailHistory();
		Update(0f);
		_damageArea.Reset(Position, isThrowingLeft, damage);
		_level.AddProjectile(_damageArea);
	}
}
