using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Raven;

internal sealed class RavenBossFlockProjectile : DamageArea
{
	private const int BboxHeight = 24;

	private const int BboxWidth = 24;

	private const int FlockSize = 3;

	private const int TravelRefreshTresholdDistance = 4096;

	private const int FlockRadius = 12;

	private const int FlockOffsetY = 8;

	private const float FlockFrequency = 5f;

	private const float FlockOscillationOffset = (float)Math.PI * 2f / 3f;

	private const float TravelSpeedNormal = 200f;

	private const float TravelSpeedHyper = 250f;

	private const float TimeBetweenRetargets = 0.05f;

	private const float MaxLife = 100f;

	private const float FlapAnimationSpeed = 0.1f;

	private static readonly Color OutlineColor = new Color(80, 0, 80, 128);

	private readonly RavenBossFeathersParticleSystem _featherParticles;

	private readonly Appendage[] _flock = new Appendage[3];

	private bool _isHyper;

	private bool _isReturning;

	private bool _isRefreshingTarget;

	private float _retargetTimer;

	private float _flockOscillationTimer;

	private Vector2 _targetVelocity;

	private Mobile _targetMobile;

	private Mobile _parentMobile;

	internal bool IsFinished { get; private set; }

	public RavenBossFlockProjectile(Level inLevel, Point inPosition, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 12, inPosition.Y - 12, 24, 24);
		_bboxOffset = new Point(4, 4);
		DrawOrigin = new Vector2(8f, 8f);
		_power = (int)Math.Ceiling(1.1f * (float)baseDamage);
		_force = 0;
		_life = 100f;
		_doesDieOnTiles = true;
		base.DoesCollideWithTiles = true;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = false;
		base.DoesDieToEnemyProjectiles = false;
		base.DoesDieOnImpact = false;
		_isFlying = true;
		_doesRotateBasedOnVelocity = false;
		_animationSpeed = 0f;
		_maxMoveSpeed = 300f;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		ChangeAnimation(-1);
		_featherParticles = new RavenBossFeathersParticleSystem(_sprite, 1, 6, 4);
		for (int i = 0; i < 3; i++)
		{
			Appendage appendage = new Appendage(this, new Point(12, 12), new Point(9, 17), _level, _sprite)
			{
				DoesDrawTrail = true,
				TrailLength = 8,
				TrailFadeRate = 4f
			};
			Appendage appendage2 = new Appendage(appendage, new Point(32, 40), Point.Zero, _level, _sprite)
			{
				DrawColor = OutlineColor,
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(1, 11),
				DoesInheritDrawColor = false
			};
			float preSpeed = (float)i / 3f * 0.1f;
			appendage.ChangeAnimation(2, 4, 0.1f, EAnimationType.Cycle, 2 + i, 4 - i, preSpeed);
			appendage2.ChangeAnimation(17, 4, 0.1f, EAnimationType.Cycle, 17 + i, 4 - i, preSpeed);
			appendage.AddAppendage(appendage2);
			base.Appendages.Add(appendage);
			_flock[i] = appendage;
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateArc(delta);
			UpdateFlock(delta);
		}
		base.Update(delta);
	}

	private void UpdateArc(float delta)
	{
		_retargetTimer -= delta;
		if (_retargetTimer <= 0f)
		{
			_retargetTimer += 0.05f;
			float num = (_isHyper ? 250f : 200f);
			if (_isRefreshingTarget)
			{
				_targetPosition = (_isReturning ? _parentMobile.Bbox.Center : _targetMobile.Bbox.Center);
			}
			Vector2 vector = new Vector2(_targetPosition.X - _position.X, _targetPosition.Y - _position.Y);
			float num2 = vector.LengthSquared();
			if (_isRefreshingTarget && num2 <= 4096f)
			{
				_isRefreshingTarget = false;
			}
			if (num2 < 256f)
			{
				if (_isReturning)
				{
					SilentKill();
				}
				else
				{
					_isReturning = true;
					_isRefreshingTarget = true;
				}
			}
			else if (Math.Abs(vector.Y) > 0f && Math.Abs(vector.X) > 0f)
			{
				vector.Normalize();
				_targetVelocity = vector * num;
			}
		}
		_velocity = _velocity.Lerp(_targetVelocity, 0.05f);
	}

	private void UpdateFlock(float delta)
	{
		IsFacingLeft = _velocity.X <= 0f;
		float num = delta * 5f;
		_flockOscillationTimer += (IsFacingLeft ? (0f - num) : num);
		if (_flockOscillationTimer > (float)Math.PI * 2f)
		{
			_flockOscillationTimer -= (float)Math.PI * 2f;
		}
		if (_flockOscillationTimer < 0f)
		{
			_flockOscillationTimer += (float)Math.PI * 2f;
		}
		Point point = new Point(Position.X, Position.Y + 8);
		for (int i = 0; i < 3; i++)
		{
			float num2 = _flockOscillationTimer + (float)Math.PI * 2f / 3f * (float)i;
			int num3 = (int)Math.Ceiling(Math.Cos(num2) * 12.0);
			int num4 = (int)Math.Ceiling(Math.Sin(num2) * 12.0);
			_flock[i].Position = new Point(point.X + num3, point.Y + num4);
		}
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	private void AddFeathers()
	{
		BattleAnimation battleAnimation = new BattleAnimation(_sprite, _bbox.Center, _level);
		battleAnimation.TeamSide = base.DefaultTeam;
		battleAnimation.AnimationStart = 10;
		battleAnimation.AnimationLength = 6;
		battleAnimation.AnimationSpeed = 0.04f;
		battleAnimation.DrawColor = Color.White * 0.8f;
		battleAnimation.ParticleSystem = _featherParticles;
		BattleAnimation newAnimation = battleAnimation;
		_level.AddAnimation(newAnimation);
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		AddFeathers();
	}

	public void Reset(Point startPoint, Mobile target, Mobile parent, bool isHyper)
	{
		_isFading = false;
		_fadeTimer = 0f;
		_life = 100f;
		IsFinished = false;
		_isHyper = isHyper;
		_isReturning = false;
		_isRefreshingTarget = true;
		Position = startPoint;
		_targetMobile = target;
		_parentMobile = parent;
		Point center = _targetMobile.Bbox.Center;
		_startPosition = startPoint;
		Vector2 vector = new Vector2(center.X - startPoint.X, center.Y - startPoint.Y);
		vector.Normalize();
		Vector2 vector2 = ((vector.X < 0f) ? new Vector2(0f - vector.Y, vector.X) : new Vector2(vector.Y, 0f - vector.X));
		_velocity = vector2 * 250f;
		_targetVelocity = _velocity;
		SnapBboxToPosition();
		base.DrawColor = Color.White;
		UpdateArc(0f);
	}
}
