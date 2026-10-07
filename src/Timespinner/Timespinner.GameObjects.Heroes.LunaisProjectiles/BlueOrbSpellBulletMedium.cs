using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class BlueOrbSpellBulletMedium : LunaisBaseProjectile
{
	private const int Width = 32;

	private const int Height = 11;

	private const float MaxLife = 1.2f;

	private readonly bool _isFirey;

	private readonly ParticleSystem _trailParticles;

	internal bool IsFinished { get; private set; }

	public BlueOrbSpellBulletMedium(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, float dormantTime, bool isFirey, int spellDamage, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, iV, inSide, dormantTime, 0, parentOrb)
	{
		_isFirey = isFirey;
		_sprite = (_isFirey ? _level.GCM.SpOrbMeleeFire : _level.GCM.SpOrbMeleeBlue);
		_bbox = new Rectangle(inPosition.X - 16, inPosition.Y - 5, 32, 11);
		DrawOrigin = new Vector2(16f, 5.5f);
		_power = spellDamage;
		_force = 2;
		_life = 1.2f;
		_damageElement = (_isFirey ? EDamageElement.Fire : EDamageElement.Aura);
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = _teamSide != ETeamSide.Enemies;
		_trailParticles = new LunaisBullet1ParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_particleSystems.Add(_trailParticles);
		ChangeAnimation(4, 4, 0.05f, EAnimationType.Cycle);
		if (_isFirey)
		{
			_trailParticles.BaseColor = Color.Red.ToVector4();
		}
		_doesDrawTrail = true;
		_trailFadeRate = 1.25f;
		_normalTrailLength = 2;
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
					position.X += 2;
				}
				else
				{
					position.X -= 2;
				}
				_level.AddAnimation(EBattleAnimationType.BigHit, position, _teamSide, isFacingRight, !_isFirey);
				if (_isFirey)
				{
					_level.PlayCue(ESFX.LunaisOrbImpactBurn, deathPoint);
				}
			}
			else
			{
				_level.AddAnimation(EBattleAnimationType.MediumHit, position, _teamSide, isFacingRight: false);
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

	internal bool Reset(Point position, Vector2 iV, float sleepTime, int damage)
	{
		_initialVector = iV;
		_velocity = iV;
		Position = position;
		ClearTrailHistory();
		_trailParticles.KillOffParticles(0.1f);
		SnapBboxToPosition();
		_power = damage;
		_dormantTimer = sleepTime;
		_isFading = false;
		_life = 1.2f;
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
