using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal sealed class NetherOrbSpellProjectile : LunaisBaseProjectile
{
	private const int BboxHeight = 6;

	private const int BboxWidth = 6;

	private const float MaxLife = 3f;

	private const float TimeBetweenParticleEmissions = 0.033f;

	private const int ExpandRadius = 48;

	private const int DefaultSeekDistance = 512;

	private const float RotationFrequency = 1f;

	private const float TimeToExpand = 0.5f;

	private const float TimeBeforeSeeking = 2f;

	private const float TimeToSeek = 1.5f;

	private const float FinalSpeed = 1000f;

	private static readonly Color SoulTrailColor = new Color(0.5f, 0.9f, 0.75f, 0.5f);

	private static readonly Vector4 SoulParticlesColor = new Vector4(0.25f, 0.66f, 0.5f, 0.5f);

	private readonly LunaisObj _parentLunais;

	private readonly SoulTrailParticleSystem _trailParticles;

	private readonly Action _playSeekAction;

	private int _idleRadius;

	private float _expandTimer;

	private float _rotateTimer;

	private float _idleTimer;

	private float _seekTimer;

	private float _particleEmissionTimer;

	private Vector2 _targetVelocity;

	internal bool IsFinished { get; set; }

	internal float StartingAngle { get; set; }

	public NetherOrbSpellProjectile(Level inLevel, Point inPosition, ETeamSide inSide, SpriteSheet sprite, LunaisSpell parentSpell, LunaisObj parentLunais, Action playSeekAction)
		: base(inLevel, inPosition, Vector2.Zero, inSide, 0, parentSpell)
	{
		_parentLunais = parentLunais;
		_playSeekAction = playSeekAction;
		_sprite = sprite;
		_doesDrawBaseSprite = false;
		_bbox = new Rectangle(inPosition.X - 3, inPosition.Y - 3, 6, 6);
		_bboxOffset = new Point(3, 1);
		DrawOrigin = new Vector2(6f, 3.5f);
		_force = 0;
		_life = 3f;
		_doesDieOnTiles = false;
		base.DoesCollideWithTiles = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = false;
		base.DoesDieToEnemyProjectiles = false;
		base.DoesDieOnImpact = true;
		_isFlying = true;
		_animationSpeed = 0f;
		_maxMoveSpeed = 1000f;
		ChangeAnimation(-1);
		_isTrailLengthAffectedByTime = false;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 8;
		_trailLength = 75;
		_trailColor = SoulTrailColor;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 10;
		_trailShrinkRate = 0.00033f;
		_trailParticles = new SoulTrailParticleSystem(_level.GCM.TxParticleEnergy, 16, SoulParticlesColor);
		_particleSystems.Add(_trailParticles);
	}

	public override void Update(float delta)
	{
		_particleEmissionTimer += delta;
		if (_particleEmissionTimer >= 0.033f)
		{
			_particleEmissionTimer -= 0.033f;
			_trailParticles.AddParticles(Bbox.Center.ToVector2());
		}
		if (_expandTimer < 0.5f)
		{
			_expandTimer += delta;
			float num = 1f;
			if (_expandTimer < 0.5f)
			{
				num = MathEx.SineInterpolate(0f, 1f, _expandTimer / 0.5f);
			}
			_idleRadius = (int)Math.Round(num * 48f);
		}
		if (_idleTimer < 2f)
		{
			_doesDieOutsideOfVisibleArea = false;
			_idleTimer += delta;
			_rotateTimer += delta * 1f;
			if (_rotateTimer >= (float)Math.PI * 2f)
			{
				_rotateTimer -= (float)Math.PI * 2f;
			}
			Point center = _parentLunais.Bbox.Center;
			float num2 = _rotateTimer + StartingAngle;
			int num3 = (int)Math.Round(Math.Cos(num2) * (double)_idleRadius);
			int num4 = (int)Math.Round(Math.Sin(num2) * (double)_idleRadius);
			Position = new Point(center.X + num3, center.Y + num4);
			if (_idleTimer >= 2f)
			{
				Monster nearestEnemy = _level.GetNearestEnemy(Position, shouldBeVisible: true, shouldBeAggroed: false);
				if (nearestEnemy != null)
				{
					_targetPosition = nearestEnemy.OuterBbox.Center;
				}
				else
				{
					_targetPosition = new Point(Position.X + num3 * 512, Position.Y + num4 * 512);
				}
				_playSeekAction();
				RecalcuateTargetVelocity();
			}
		}
		else
		{
			_doesDieOutsideOfVisibleArea = true;
			if (_seekTimer < 1.5f)
			{
				RecalcuateTargetVelocity();
				_seekTimer += delta;
				if (_seekTimer < 1.5f)
				{
					float amount = (float)Math.Sin(_seekTimer / 1.5f * ((float)Math.PI / 2f));
					_velocity = _initialVector.SineInterpolate(_targetVelocity, amount);
				}
				else
				{
					_velocity = _targetVelocity;
				}
			}
			else
			{
				_velocity = _targetVelocity;
			}
			if (IsOutsideOfLevel())
			{
				SilentKill();
			}
		}
		base.Update(delta);
	}

	public override void Kill()
	{
		_level.AddAnimation(EBattleAnimationType.SmallBoom, _bbox.Center, _teamSide);
		base.Kill();
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	private void RecalcuateTargetVelocity()
	{
		Vector2 vector = new Vector2(_targetPosition.X - Position.X, _targetPosition.Y - Position.Y);
		float num = vector.LengthSquared();
		if (num < 256f)
		{
			_seekTimer = 10f;
			return;
		}
		vector.Normalize();
		_targetVelocity = vector * 1000f;
	}

	internal void Reset(Point newPosition, int power, float angle)
	{
		_seekTimer = 0f;
		_initialVector = Vector2.Zero;
		_velocity = Vector2.Zero;
		Position = newPosition;
		StartingAngle = angle;
		ClearTrailHistory();
		SnapBboxToPosition();
		_power = power;
		base.ID = -1;
		_isFading = false;
		_life = 3f;
		IsFinished = false;
		_isFading = false;
		_canDamageThings = true;
		_fadeTimer = 0f;
		_idleTimer = 0f;
		_expandTimer = 0f;
		_rotateTimer = 0f;
	}
}
