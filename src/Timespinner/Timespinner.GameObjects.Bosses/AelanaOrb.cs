using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Bosses;

internal sealed class AelanaOrb : Animate
{
	private const EInventoryOrbType Color = EInventoryOrbType.Pink;

	private const int OrbAnimationIndexStart = 48;

	private const float OrbIdleAnimationSpeed = 0.05f;

	private const float MaxLeashLength = 100f;

	private const float LeashShrinkRate = 7.5f;

	private const float IdleRadiusGrowthFactor = 10f;

	private const float MaxIdleRadius = 8f;

	private const float IdleTimeFactor = 4f;

	private const float IdleXScale = 0.5f;

	private const float IdleYScale = 0.75f;

	private static readonly Vector2 IdleOrigin = new Vector2(5f, 5f);

	private readonly bool _isMainOrb;

	private readonly AelanaOrbDamageArea _orbDamageArea;

	private readonly LunaisChargeLazerParticleSystem _chargeLazerParticles;

	private readonly LunaisChargeParticleSystem _chargeSparkleParticles;

	private readonly AelanaBoss _parentObject;

	private bool _isOrbFacingLeft;

	private bool _isSpinning;

	private int _deathBounceCount;

	private EOrbState _state;

	private float _distToTarget;

	private float _leashLength;

	private float _leashTimeSpentNotCentered;

	private float _xCosMult;

	private float _xScale = 1f;

	private float _yScale = 1f;

	private float _orbXShift;

	private float _oscillDelta;

	private float _oscillRadius;

	private Point _curTarget;

	private Vector2 _basePosition;

	private Vector2 _leashVector;

	private SFXCueInstance _lightningBallLoopCue;

	internal bool ShouldAddChargeParticles { get; set; }

	public EOrbState State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
		}
	}

	internal Vector2 BasePosition => _basePosition;

	public AelanaOrb(Level inLevel, Point inPosition, SpriteSheet inSprite, AelanaBoss inParent, bool isMainOrb, int baseDamage)
		: base(inPosition, inLevel, 4)
	{
		_isMainOrb = isMainOrb;
		_defaultTeam = ETeamSide.Enemies;
		_parentObject = inParent;
		_targetPosition = _parentObject.FindBulletOffset();
		_curTarget = _targetPosition;
		_basePosition = new Vector2(_targetPosition.X, _targetPosition.Y);
		_state = EOrbState.Idle;
		_sprite = inSprite;
		_position = inPosition;
		_bbox = new Rectangle(inPosition.X - 4, inPosition.Y - 4, 8, 8);
		_bboxOffset = new Point(1, 1);
		DrawOrigin = IdleOrigin;
		_glowBase = 1.5f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = true;
		_isFlying = true;
		_chargeSparkleParticles = new LunaisChargeParticleSystem(_level.GCM.TxParticleEnergy, 2);
		_chargeLazerParticles = new LunaisChargeLazerParticleSystem(_level.GCM.TxParticleEnergy, 2)
		{
			StartRadiusOffset = 5f
		};
		_particleSystems.Add(_chargeSparkleParticles);
		_particleSystems.Add(_chargeLazerParticles);
		ChangeAnimation(48, 10, 0.05f, EAnimationType.Cycle);
		_trailFadeRate = 1f;
		_trailLength = 50;
		_trailInterpolationAmount = 5;
		_trailShrinkRate = 0.0005f;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		Vector4 baseColor = new Vector4(1f, 0.4f, 0.6f, 1f);
		_trailColor = new Color(0.75f, 0.25f, 0.5f, 0.1f);
		_chargeSparkleParticles.BaseColor = baseColor;
		_chargeSparkleParticles.BaseColor = baseColor;
		_chargeLazerParticles.BaseColor = baseColor;
		_orbDamageArea = new AelanaOrbDamageArea(_level, Position, base.DefaultTeam, this, baseDamage);
		_level.AddProjectile(_orbDamageArea);
		if (!_isMainOrb)
		{
			_oscillDelta = (float)Math.PI;
		}
	}

	public void Update(float delta, Point targetPoint, bool isParentFacingLeft, Vector2 inVelocity)
	{
		_isFrozen = _parentObject.IsFrozen;
		if (_isFrozen)
		{
			return;
		}
		_curTarget = targetPoint;
		_isOrbFacingLeft = isParentFacingLeft;
		_oscillDelta += delta * 4f;
		if (_oscillDelta > 6.28f)
		{
			_oscillDelta -= 6.28f;
		}
		UpdateLeash(targetPoint, delta);
		switch (State)
		{
		case EOrbState.Idle:
			OrbIdle(delta);
			break;
		case EOrbState.Charge:
			OrbCharge(delta);
			break;
		case EOrbState.Dead:
			base.Rotation += _velocity.X / 500f;
			_animationSpeed += 0.01f;
			break;
		}
		if (base.IsAnimationQueueEmpty)
		{
			switch (_state)
			{
			case EOrbState.Idle:
				_animationSpeed = 0.1f;
				break;
			case EOrbState.Charge:
				_animationSpeed = 0.01f;
				break;
			}
		}
		Update(delta);
		if (_state != EOrbState.Dead)
		{
			return;
		}
		_doesDrawTrail = false;
		foreach (Point intermediatePosition in base.IntermediatePositions)
		{
			_position = intermediatePosition;
			SnapBboxToPosition();
			if (DetectTileCollisions())
			{
				break;
			}
		}
	}

	private void OrbIdle(float delta)
	{
		CircleInPlace();
		if (_oscillRadius < 8f)
		{
			_oscillRadius += delta * 10f;
			if (_oscillRadius > 8f)
			{
				_oscillRadius = 8f;
			}
		}
		if (_orbXShift > 0f)
		{
			_orbXShift -= delta * 10f;
			if (_orbXShift < 0f)
			{
				_orbXShift = 0f;
			}
		}
	}

	private void OrbCharge(float delta)
	{
		CircleInPlace();
		if (_oscillRadius > 0f)
		{
			_oscillRadius -= delta * 10f;
			if (_oscillRadius < 0f)
			{
				_oscillRadius = 0f;
			}
		}
		if (_orbXShift > 0f)
		{
			_orbXShift -= delta * 10f;
			if (_orbXShift < 0f)
			{
				_orbXShift = 0f;
			}
		}
		if (_isMainOrb && ShouldAddChargeParticles)
		{
			_chargeSparkleParticles.AddParticles(new Vector2(_position.X, _position.Y));
			_chargeLazerParticles.AddParticles(new Vector2(_position.X, _position.Y));
		}
	}

	private void CircleInPlace()
	{
		float num = _orbXShift * (float)(_isOrbFacingLeft ? 1 : (-1));
		float num2 = _leashVector.X * _leashLength;
		float num3 = _leashVector.Y * _leashLength;
		_basePosition = new Vector2((float)_curTarget.X + num2, (float)_curTarget.Y + num3);
		if (_state == EOrbState.Idle || _state == EOrbState.Charge)
		{
			_xScale = 0.5f;
			_yScale = 0.75f;
		}
		_xCosMult = (float)(Math.Cos(_oscillDelta) * (double)_oscillRadius * (double)_xScale);
		Position = new Point((int)Math.Round(_xCosMult + _basePosition.X + num), (int)Math.Round(Math.Sin(_oscillDelta) * (double)_oscillRadius * (double)_yScale + (double)_basePosition.Y));
	}

	private void UpdateLeash(Point targetPoint, float delta)
	{
		Vector2 value = new Vector2(_basePosition.X - (float)targetPoint.X, _basePosition.Y - (float)targetPoint.Y);
		_leashLength = 0f;
		_distToTarget = value.Length();
		if (_distToTarget != 0f)
		{
			_leashLength = ((_distToTarget > 100f) ? 100f : _distToTarget);
			_leashVector = Vector2.Normalize(value);
			if (_leashTimeSpentNotCentered < 2.5f)
			{
				_leashTimeSpentNotCentered += delta;
			}
		}
		else
		{
			_leashTimeSpentNotCentered = 0f;
		}
		if (_leashLength > 0f)
		{
			_leashLength -= 7.5f * delta * _leashLength;
			if (_leashLength < 0f)
			{
				_leashLength = 0f;
			}
		}
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height / 2);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_doesDrawTrail)
		{
			DrawTrail(spriteBatch);
		}
		_doesDrawTrail = false;
		base.Draw(spriteBatch);
		_doesDrawTrail = true;
	}

	public void Kill(bool isMainOrb)
	{
		_state = EOrbState.Dead;
		_isAffectedByGravity = true;
		_isAffectedByFriction = true;
		_doesOverrideMaxSpeed = false;
		_doesBounceOnGround = true;
		_isFlying = true;
		_gravityAcceleration = 350f;
		_airDragFactor = 0.015f;
		_velocity = new Vector2((150 + (isMainOrb ? 50 : 0)) * (_isOrbFacingLeft ? 1 : (-1)), -150f + _oscillDelta);
	}

	protected override void DoBounce(Point impactPoint)
	{
		if (_state == EOrbState.Dead && _deathBounceCount < 3)
		{
			_deathBounceCount++;
			PlayCue(ESFX.BossSorceressDeathOrbBounce, impactPoint);
		}
		base.DoBounce(impactPoint);
	}

	internal void DoDeathCleanup(bool isMainOrb)
	{
		if (_isSpinning)
		{
			StopSpinning();
		}
		_orbDamageArea.HasInfiniteLife = false;
		_orbDamageArea.Kill();
		if (_lightningBallLoopCue != null)
		{
			_lightningBallLoopCue.Stop();
		}
		Kill(isMainOrb);
	}

	internal void StartSpinning()
	{
		_isSpinning = true;
		_orbDamageArea.CanDamageThings = true;
		if (_lightningBallLoopCue == null)
		{
			_lightningBallLoopCue = CreateCue(ESFX.BossSorceressLightningBallLoop, Position, isLooped: true);
			if (_lightningBallLoopCue != null)
			{
				_lightningBallLoopCue.Anchor = this;
				_lightningBallLoopCue.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Anchor;
				_lightningBallLoopCue.Play();
			}
		}
		else
		{
			_lightningBallLoopCue.Resume();
		}
	}

	internal void StopSpinning()
	{
		_isSpinning = false;
		if (_lightningBallLoopCue != null)
		{
			_lightningBallLoopCue.Pause(0.1f);
		}
		_orbDamageArea.CanDamageThings = false;
	}
}
