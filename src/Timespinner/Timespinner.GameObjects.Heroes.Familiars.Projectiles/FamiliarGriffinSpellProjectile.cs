using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars.Projectiles;

internal sealed class FamiliarGriffinSpellProjectile : FamiliarBaseProjectile
{
	private const int BboxHeight = 12;

	private const int BboxWidth = 12;

	private const float MaxLife = 1.5f;

	private const float FinalSpeed = 600f;

	private const float TimeToSeek = 1.5f;

	private static readonly Color BaseDrawColor = Color.White * 0.75f;

	private float _seekTimer;

	private Vector2 _targetVelocity;

	internal bool IsFinished { get; set; }

	public FamiliarGriffinSpellProjectile(Level inLevel, Point inPosition, ETeamSide inSide, SpriteSheet sprite, FamiliarBase parentFamiliar)
		: base(inLevel, inPosition, Vector2.Zero, inSide, 0f, parentFamiliar)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 6, inPosition.Y - 6, 12, 12);
		_bboxOffset = new Point(0, 2);
		DrawOrigin = new Vector2(6f, 8f);
		_force = 0;
		_life = 1.5f;
		_damageElement = EDamageElement.Sharp;
		_doesDieOnTiles = false;
		base.DoesCollideWithTiles = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = false;
		base.DoesDieToEnemyProjectiles = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesDieOnImpact = true;
		_doesRotateBasedOnVelocity = true;
		base.DrawColor = BaseDrawColor;
		_animationSpeed = 0f;
		_maxMoveSpeed = 600f;
		ChangeAnimation(32);
		_isTrailLengthAffectedByTime = false;
		_doesDrawTrail = true;
		_trailLength = 4;
		_trailFadeRate = 2f;
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

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		Point position = FindDeathPoint(target, collidingRectangle);
		_level.AddAnimation(EBattleAnimationType.SmallHit, position, _teamSide, isFacingRight: false, doesPlaySFX: false);
		_level.PlayCue(ESFX.LunaisPiercingHit, position);
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
		base.DrawColor = BaseDrawColor;
	}
}
