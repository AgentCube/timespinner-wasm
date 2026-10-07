using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal sealed class GunOrbSpellProjectile : LunaisBaseProjectile
{
	private const int BboxHeight = 6;

	private const int BboxWidth = 6;

	private const float MaxLife = 1.5f;

	private const float TimeToSeek = 1.5f;

	private const float FinalSpeed = 600f;

	private float _seekTimer;

	private Vector2 _targetVelocity;

	internal bool IsFinished { get; set; }

	public GunOrbSpellProjectile(Level inLevel, Point inPosition, ETeamSide inSide, SpriteSheet sprite, LunaisSpell parentSpell)
		: base(inLevel, inPosition, Vector2.Zero, inSide, 0, parentSpell)
	{
		_sprite = sprite;
		_doesDrawBaseSprite = false;
		_bbox = new Rectangle(inPosition.X - 3, inPosition.Y - 3, 6, 6);
		_bboxOffset = new Point(3, 1);
		DrawOrigin = new Vector2(6f, 3.5f);
		_force = 0;
		_life = 1.5f;
		_doesDieOnTiles = true;
		base.DoesCollideWithTiles = true;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = false;
		base.DoesDieToEnemyProjectiles = true;
		_isFlying = true;
		_damageElement = EDamageElement.Light;
		_animationSpeed = 0f;
		_maxMoveSpeed = 600f;
		ChangeAnimation(27);
		_doesDrawParticleSystemsUnder = true;
		_isTrailLengthAffectedByTime = false;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 8;
		_trailLength = 75;
		_trailColor = new Color(0.9f, 0.8f, 0.85f, 0.5f);
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 10;
		_trailShrinkRate = 0.00033f;
	}

	public override void Update(float delta)
	{
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
		_targetVelocity = vector * 600f;
	}

	internal void Reset(Vector2 iV, Point newPosition, Point newTarget, int power)
	{
		_seekTimer = 0f;
		_initialVector = iV;
		_velocity = iV;
		Position = newPosition;
		_targetPosition = newTarget;
		RecalcuateTargetVelocity();
		ClearTrailHistory();
		SnapBboxToPosition();
		_power = power;
		base.ID = -1;
		_isFading = false;
		_life = 1.5f;
		IsFinished = false;
		_isFading = false;
		_canDamageThings = true;
		_fadeTimer = 0f;
	}
}
