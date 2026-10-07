using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Zel;

internal sealed class ZelBossSwordAppendage : Appendage
{
	private const int SwordFrameIndex = 48;

	private const int SwordOffsetX = -48;

	private const int SwordOffsetY = -32;

	private const int SwordSwipeDamageMinSize = 16;

	private const int SwordSwipeDamageOffsetX = 64;

	private const int SwordSwipeDamageOffsetY = 64;

	private const int SwordSwipeDamageRadius = 120;

	private const float TimeForSwordAppear = 1f;

	private const float TimeForSwordSwipe = 0.125f;

	private const float TimeForSwordSlowDown = 0.1f;

	private const float TimeForSwordFadeOut = 0.5f;

	private const float TimeBeforeSwordSwipe = 1f;

	private const float TimeBeforeSwordSlowDown = 1.125f;

	private const float TimeBeforeFadingOut = 1.225f;

	private const float TimeForTotalAttack = 1.725f;

	private const float SwordAppearAngle = (float)Math.PI / 2f;

	private const float SwordSwipeStartAngle = 2.8561945f;

	private const float SwordSwipeEndAngle = 1.2853982f;

	private const float SwordSwipeSlowDownAngle = 1.0353982f;

	private readonly ZelBossSwordDamageArea _damageArea;

	private bool _isThrowingLeft;

	private float _attackTimer;

	internal bool IsFinished { get; set; }

	internal bool IsAvailable { get; set; }

	internal Point CurrentTarget { get; set; }

	public ZelBossSwordAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		ChangeAnimation(48);
		_damageArea = new ZelBossSwordDamageArea(_level, Position, ETeamSide.Enemies, 0, _sprite);
		IsAvailable = true;
		base.DoesDrawTrail = true;
		base.TrailLength = 12;
		base.TrailFadeRate = 2f;
		base.TimeToTurnAround = 0f;
		base.DoesInheritDrawColor = false;
		base.IsFacingLocked = true;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			Position = new Point(CurrentTarget.X + (_isThrowingLeft ? (-48) : 48), CurrentTarget.Y + -32);
			if (!IsFinished)
			{
				float attackTimer = _attackTimer;
				_attackTimer += delta;
				if (_attackTimer < 1.725f)
				{
					if (attackTimer <= 0f)
					{
						PlayCue(ESFX.BossZelSword);
					}
					if (_attackTimer < 1f)
					{
						float num = _attackTimer / 1f;
						float angle = MathEx.SineInterpolate((float)Math.PI / 2f, 2.8561945f, num);
						SetSwordAngle(angle, isDamageAreaActive: false);
						base.IsGlowing = true;
						base.GlowColor = new Color(1f, 1f, 1f, num);
						base.GlowBase = MathEx.SineInterpolate(16f, 0f, num);
						if (num < 0.5f)
						{
							base.Scale = num * 2f;
						}
						else
						{
							base.Scale = 1f;
						}
					}
					else if (_attackTimer < 1.125f)
					{
						if (attackTimer < 1f)
						{
							base.IsGlowing = false;
							base.DrawColor = Color.White;
							base.Scale = 1f;
						}
						float percentage = (_attackTimer - 1f) / 0.125f;
						float angle2 = MathEx.CosInterpolate(2.8561945f, 1.2853982f, percentage);
						SetSwordAngle(angle2, isDamageAreaActive: true);
					}
					else if (_attackTimer < 1.225f)
					{
						float percentage2 = (_attackTimer - 1.125f) / 0.1f;
						float angle3 = MathEx.SineInterpolate(1.2853982f, 1.0353982f, percentage2);
						SetSwordAngle(angle3, isDamageAreaActive: true);
					}
					else
					{
						if (attackTimer < 1.225f)
						{
							SetSwordAngle(1.0353982f, isDamageAreaActive: false);
						}
						float num2 = (_attackTimer - 1.225f) / 0.5f;
						base.DrawColor = Color.White * (1f - num2);
					}
				}
				else
				{
					IsFinished = true;
					_damageArea.SilentKill();
				}
			}
		}
		base.Update(delta);
	}

	private void SetSwordAngle(float angle, bool isDamageAreaActive)
	{
		if (_isThrowingLeft)
		{
			base.Rotation = angle;
		}
		else
		{
			base.Rotation = 0f - angle;
		}
		if (isDamageAreaActive)
		{
			_damageArea.CanDamageThings = true;
			float num = angle - (float)Math.PI / 2f;
			double num2 = Math.Cos(num);
			double num3 = Math.Sin(num);
			int num4 = (int)Math.Ceiling(num2 * 64.0);
			int num5 = -(int)Math.Ceiling(num3 * 64.0);
			int width = MathEx.Max(16, (int)Math.Ceiling(num2 * 120.0));
			int height = MathEx.Max(16, (int)Math.Abs(Math.Ceiling(num3 * 120.0)));
			Point position = new Point(Position.X + num4 * ((!_isThrowingLeft) ? 1 : (-1)), Position.Y + num5);
			_damageArea.SetBoundingBox(position, width, height);
		}
		else
		{
			_damageArea.CanDamageThings = false;
		}
	}

	internal void Reset(Point target, bool isThrowingLeft, int damage)
	{
		IsFinished = false;
		IsAvailable = false;
		_attackTimer = 0f;
		CurrentTarget = target;
		_isThrowingLeft = isThrowingLeft;
		IsFacingLeft = isThrowingLeft;
		DrawOrigin = new Vector2(23f, 0f);
		base.DrawColor = Color.White;
		ClearTrailHistory();
		base.DoesInheritDrawColor = false;
		Update(0f);
		_damageArea.Reset(Position, isThrowingLeft, damage);
		_level.AddProjectile(_damageArea);
	}
}
