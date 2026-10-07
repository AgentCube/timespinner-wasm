using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class BlueOrbSpellBulletLarge : LunaisBaseProjectile
{
	private const int Width = 32;

	private const int Height = 24;

	private const float MaxLife = 1.5f;

	private readonly ParticleSystem _trailParticles;

	internal bool IsFinished { get; private set; }

	public BlueOrbSpellBulletLarge(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, int spellDamage, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, iV, inSide, 0f, 0, parentOrb)
	{
		_sprite = _level.GCM.SpOrbMeleeBlue;
		_bbox = new Rectangle(inPosition.X - 16, inPosition.Y - 12, 32, 24);
		DrawOrigin = new Vector2(16f, 12f);
		base.DrawColor = Color.White * 0.8f;
		_power = spellDamage;
		_force = 3;
		_life = 1.5f;
		_damageElement = EDamageElement.Aura;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = inSide != ETeamSide.Enemies;
		_trailParticles = new LunaisBullet1ParticleSystem(_level.GCM.TxParticleEnergy, 15);
		_particleSystems.Add(_trailParticles);
		ChangeAnimation(8, 4, 0.04f, EAnimationType.Cycle);
		_doesDrawTrail = true;
		_trailFadeRate = 1.5f;
		_normalTrailLength = 5;
		_trailLength = _normalTrailLength;
		_trailShrinkRate = 0.15f;
	}

	public override void Kill(bool useAnimation, Point deathPoint, bool deathFromInvulnerable)
	{
		if (useAnimation)
		{
			bool isFacingRight = true;
			Point position = deathPoint;
			if (!deathFromInvulnerable)
			{
				if (_velocity.X > 0f)
				{
					isFacingRight = false;
					position.X += 3;
				}
				else
				{
					position.X -= 3;
				}
				_level.AddAnimation(EBattleAnimationType.BigHit, position, _teamSide, isFacingRight);
			}
			else
			{
				_level.AddAnimation(EBattleAnimationType.BigHit, position, _teamSide, isFacingRight: true);
			}
		}
		Kill();
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		Kill(useAnimation: true, contactPoint, deathFromInvulnerable: true);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	public bool Reset(Point startPoint, Vector2 iV, int damage)
	{
		_initialVector = iV;
		_velocity = iV;
		Position = startPoint;
		ClearTrailHistory();
		_trailParticles.KillOffParticles(0.1f);
		SnapBboxToPosition();
		_power = damage;
		_isFading = false;
		_life = 1.5f;
		_isFading = false;
		_canDamageThings = true;
		_fadeTimer = 0f;
		base.DrawColor = Color.White;
		bool isFinished = IsFinished;
		IsFinished = false;
		if (isFinished)
		{
			base.ID = -1;
		}
		return isFinished;
	}
}
