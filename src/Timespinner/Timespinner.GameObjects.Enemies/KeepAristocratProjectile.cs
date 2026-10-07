using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class KeepAristocratProjectile : Projectile
{
	private const int BboxWidth = 8;

	private const int BboxHeight = 8;

	private const float MaxLife = 3.75f;

	private const float TimeBeforeDashing = 2f;

	private const float DashVelocity = 400f;

	private const int OrbitRadius = 24;

	private const float OrbitFrequency = 1.5f;

	internal const float TimeToStartDash = 1.75f;

	private readonly Mobile _parent;

	private bool _isDashing;

	private float _orbitTimer;

	private Vector2 _originalIV;

	public KeepAristocratProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage, Mobile parent)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_parent = parent;
		_originalIV = new Vector2((iV.X > 0f) ? 1 : (-1), 0f);
		_sprite = sprite;
		_bboxOffset = Point.Zero;
		_bbox = new Rectangle(inPosition.X - 4, inPosition.Y - 4, 8, 8);
		SnapBboxToPosition();
		_power = (int)Math.Ceiling(1.5f * (float)baseDamage);
		_force = 0;
		_life = 3.75f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_isDashing)
		{
			if (_life < 2f)
			{
				StartDash();
			}
			else if (!_isDormant)
			{
				UpdateOrbiting(delta);
			}
		}
		base.Update(delta);
	}

	internal void StartDash()
	{
		_isDashing = true;
		_velocity.X = 400f * _originalIV.X;
		_velocity.Y = -50f;
		_isAffectedByGravity = true;
		_isFlying = false;
		_airDragFactor = 0f;
		_gravityAcceleration = 100f;
		base.DoesCollideWithTiles = true;
		base.DoesDieToEnemyProjectiles = true;
		_level.AddAnimation(new BattleAnimation(_sprite, Bbox.Center, _level)
		{
			AnimationStart = 27,
			AnimationLength = 6,
			AnimationSpeed = 0.066f,
			TeamSide = _defaultTeam
		});
		ChangeAnimation(23, 4, 0.066f, EAnimationType.Cycle);
		BboxOffset = new Point(2, 0);
		PlayCue(ESFX.EnemyFireMageFireCast);
	}

	private void UpdateOrbiting(float delta)
	{
		base.Velocity = Vector2.Zero;
		_orbitTimer += delta;
		double num = Math.Cos((double)(_orbitTimer * 1.5f) * Math.PI);
		double num2 = Math.Sin((double)(_orbitTimer * 1.5f) * Math.PI);
		int x = -(int)(num * 24.0 * (double)_originalIV.X);
		int y = -(int)((0.33000001311302185 * num2 + 0.6600000262260437 * num) * 24.0);
		Point center = _parent.Bbox.Center;
		Position = center.Add(x, y);
	}

	public override void FadeKill()
	{
		if (!_isFading)
		{
			DoDeathAnimation(Bbox.Center);
		}
		base.FadeKill();
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collisionRectangle)
	{
		base.AddImpactAnimation(target, collisionRectangle);
		FadeKill();
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		DoDeathAnimation(contactPoint);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}

	internal void DoDeathAnimation(Point position)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, position, _level)
		{
			TeamSide = _defaultTeam,
			AnimationStart = 27,
			AnimationLength = 6
		});
		_level.PlayCue(ESFX.LunaisOrbImpactBurn, position);
	}

	public void Reset(Point startPoint, Vector2 iV, float dormantTime)
	{
		_isFading = false;
		_fadeTimer = 0f;
		_life = 3.75f;
		_isDashing = false;
		_orbitTimer = 0f;
		_originalIV = new Vector2((iV.X > 0f) ? 1 : (-1), 0f);
		_dormantTimer = dormantTime;
		if (dormantTime > 0f)
		{
			_isDormant = true;
		}
		Position = startPoint;
		_initialVector = iV;
		SnapBboxToPosition();
		_isAffectedByGravity = false;
		_isAffectedByFriction = true;
		_airDragFactor = 0.1f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		base.DoesDieOnImpact = true;
		base.DoesCollideWithTiles = false;
		_doesDieOnTiles = true;
		_doesCollideWithFloors = true;
		base.DoesDieToEnemyProjectiles = false;
		_bboxOffset = Point.Zero;
		_bbox = new Rectangle(startPoint.X - 4, startPoint.Y - 4, 8, 8);
		ChangeAnimation(39, 4, 0.066f, EAnimationType.Cycle);
		base.DrawColor = Color.White;
		UpdateOrbiting(0f);
	}
}
