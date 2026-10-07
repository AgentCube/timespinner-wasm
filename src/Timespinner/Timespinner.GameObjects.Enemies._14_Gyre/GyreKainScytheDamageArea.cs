using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._14_Gyre;

internal sealed class GyreKainScytheDamageArea : DamageArea
{
	private const int IdleWidth = 8;

	private const int IdleHeight = 16;

	private const int WindupWidth = 48;

	private const int ThrowWidth = 248;

	private const int FollowOffsetX = 32;

	private const int FollowOffsetY = -16;

	private const float IdleFrequency = 2.5f;

	private const float TimeToThrow = 1.25f;

	private const float TimeToWindup = 0.5f;

	private const float IdleRotationMax = (float)Math.PI / 8f;

	private const float TimeToIdleRotate = 2f;

	private const float TimeForScytheToDie = 0.75f;

	private const float IdleRotationMin = -(float)Math.PI / 4f;

	private const float ThrowRotationSpeed = 35f;

	private static readonly Color BaseAuraColor = Color.Purple * 0.5f;

	private readonly int _baseDamage;

	private readonly GyreKain _parentKain;

	private bool _isThrowing;

	private bool _isThrowingWindup;

	private bool _isThrowingRecover;

	private bool _isThrowingLeft;

	private bool _isRotatingToMax;

	private bool _isScytheDying;

	private float _idleTimer;

	private float _throwTimer;

	private float _rotationTimer;

	private float _targetRotation;

	private float _startRotation;

	private float _scytheDeathTimer;

	private Point _lastParentPosition;

	public GyreKainScytheDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, GyreKain inParent, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, inSide, -1, null)
	{
		_sprite = sprite;
		_parentKain = inParent;
		_baseDamage = baseDamage;
		_power = 0;
		Bbox = new Rectangle(0, 0, 80, 80);
		_bboxOffset = new Point(-8, 14);
		ChangeAnimation(0);
		DrawOrigin = new Vector2(29f, 54f);
		_doesRotateBasedOnVelocity = false;
		_isTrailLengthAffectedByTime = false;
		base.DoesDrawAura = true;
		base.AuraColor = BaseAuraColor;
		base.AuraSize = 0.05f;
		base.AuraFrequency = 9f;
		base.IsAffectedByTime = true;
		_doesDrawTrail = true;
		_trailFadeRate = 2f;
		_trailLength = 12;
	}

	public override void Update(float delta)
	{
		_life = 100f;
		if (!base.IsFrozen)
		{
			_idleTimer += delta * 2.5f;
			if (_idleTimer >= (float)Math.PI * 2f)
			{
				_idleTimer -= (float)Math.PI * 2f;
			}
			int num = (int)Math.Ceiling(Math.Cos(_idleTimer) * 8.0);
			int num2 = (int)Math.Ceiling(Math.Sin(_idleTimer) * 16.0);
			int num3 = 0;
			float num4 = 0f;
			if (_isThrowing)
			{
				_throwTimer += delta;
				if (_isThrowingWindup)
				{
					float num5 = 1f;
					if (_throwTimer < 0.5f)
					{
						num5 = _throwTimer / 0.5f;
					}
					num4 = (float)Math.Sin(num5 * ((float)Math.PI / 2f));
					num3 = -(int)Math.Ceiling(num4 * 48f);
					UpdateScytheRotation(delta);
				}
				else if (_isThrowingRecover)
				{
					float num6 = 1f;
					if (_throwTimer < 0.5f)
					{
						num6 = _throwTimer / 0.5f;
						base.Rotation -= delta * MathEx.SineInterpolate(35f, 0f, num6);
						if (base.Rotation < 0f)
						{
							base.Rotation += (float)Math.PI * 2f;
						}
					}
					else
					{
						_isThrowing = false;
						_isThrowingRecover = false;
						_startRotation = base.Rotation;
						_targetRotation = -(float)Math.PI / 4f;
						_isRotatingToMax = false;
						_rotationTimer = 0f;
						_power = 0;
						base.CanDamageThings = false;
					}
					num4 = 1f - (float)Math.Sin(num6 * ((float)Math.PI / 2f));
					num3 = -(int)Math.Ceiling(num4 * 48f);
				}
				else if (_throwTimer >= 1.25f)
				{
					_isThrowingRecover = true;
					_throwTimer = 0f;
					_power = 0;
					base.CanDamageThings = false;
					num4 = 1f;
					num3 = -48;
				}
				else
				{
					float num7 = _throwTimer / 1.25f;
					num4 = (float)Math.Sin(num7 * (float)Math.PI);
					num3 = (int)Math.Ceiling(num4 * 248f);
					base.Rotation -= delta * 35f;
					if (base.Rotation < 0f)
					{
						base.Rotation += (float)Math.PI * 2f;
					}
				}
			}
			else
			{
				UpdateScytheRotation(delta);
			}
			Point point = (_isScytheDying ? _lastParentPosition : (_lastParentPosition = _parentKain.Position));
			Point position = new Point(point.X + 32 + num, point.Y + -16 + num2);
			if (!_isThrowing)
			{
				Position = position;
			}
			else
			{
				int num8 = ((!_isThrowingWindup && !_isThrowingRecover) ? (-48) : 0);
				int num9 = (int)Math.Ceiling(MathHelper.Lerp(num8, num3, num4));
				int num10 = (int)Math.Ceiling(MathHelper.Lerp(num2, 0f, num4));
				Position = new Point(point.X + 32 + num + num9 * ((!_isThrowingLeft) ? 1 : (-1)), point.Y + -16 + num10);
			}
			if (_isScytheDying)
			{
				_scytheDeathTimer += delta;
				if (_scytheDeathTimer >= 0.75f)
				{
					SilentKill();
				}
				else
				{
					float amount = _scytheDeathTimer / 0.75f;
					base.DrawColor = Color.White.CosInterpolate(Color.Transparent, amount);
					base.AuraColor = BaseAuraColor.CosInterpolate(Color.Transparent, amount);
				}
			}
		}
		base.Update(delta);
	}

	private void UpdateScytheRotation(float delta)
	{
		_rotationTimer += delta;
		if (_rotationTimer >= 2f)
		{
			base.Rotation = _targetRotation;
			_startRotation = _targetRotation;
			_targetRotation = (_isRotatingToMax ? (-(float)Math.PI / 4f) : ((float)Math.PI / 8f));
			_isRotatingToMax = !_isRotatingToMax;
			_rotationTimer = 0f;
		}
		else
		{
			float percentage = _rotationTimer / 2f;
			base.Rotation = (_isRotatingToMax ? MathEx.SineInterpolate(_startRotation, _targetRotation, percentage) : MathEx.CosInterpolate(_startRotation, _targetRotation, percentage));
		}
	}

	internal void DoThrowWindup(bool isThrowingLeft)
	{
		_isThrowing = true;
		_isThrowingWindup = true;
		_isThrowingRecover = false;
		_throwTimer = 0f;
		_isThrowingLeft = isThrowingLeft;
	}

	internal void DoThrow(bool isThrowingLeft)
	{
		PlayCue(ESFX.EnemyKainAttackThrow);
		_isThrowing = true;
		_isThrowingWindup = false;
		_isThrowingRecover = false;
		_throwTimer = 0f;
		_isThrowingLeft = isThrowingLeft;
		_power = _baseDamage;
		base.CanDamageThings = true;
	}

	internal void KillScythe()
	{
		_isScytheDying = true;
		_power = 0;
	}
}
