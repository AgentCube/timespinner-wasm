using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CursedMushroomTower : Monster
{
	private const int MaxSimultaneousSaplings = 16;

	private const int MinionLocationJitterRadius = 12;

	private const int ShakeAnimationLength = 7;

	private const float ShakeMagnitudePerHit = 5f;

	private const float ShakeMagnitudeRateOfDecay = 10f;

	private const float MaxShakeMagnitude = 20f;

	private const float TimeBetweenHitSFXEmission = 0.1f;

	private const float TimeBetweenSproutsSpawning = 0.25f;

	private const int DamageAreaMinWidth = 56;

	private const float DamageAreaWidthMultiplier = 4f;

	private const float EmissionWidthMultiplier = 1f;

	private const float ShakeBaseAnimationSpeed = 0.125f;

	private const float ShakeMinAnimationSpeed = 0.03f;

	private const float ShakeAnimationRateOfDecay = 0.1f;

	private readonly CursedMushroomSporeParticleSystem _sporeParticleSystem;

	private readonly CursedMushroomSporeDamageArea _sporeDamageArea;

	private readonly ObjectTileSpecification _saplingObjectSpec;

	private readonly MinionContainer _minionContainer;

	private bool _isShaking;

	private bool _hasBeenTouched;

	private float _shakeMagnitude;

	private float _hitSfxEmissionTimer;

	private float _sproutTimer;

	private Point _lastDamagePoint;

	private Vector2 _lastDamageVector;

	public CursedMushroomTower(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		Position = inPosition.Add(8, 0);
		_currentAI = EAIStrategy.None;
		_bboxOffset = new Point(4, 8);
		Bbox = new Rectangle(_position.X, _position.Y, 40, 56);
		base.DoesTouchDamageKnockback = true;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		ChangeAnimation(0);
		Color color = new Color(180, 80, 160) * 0.75f;
		_sporeParticleSystem = new CursedMushroomSporeParticleSystem(_sprite, 10, color.ToVector4());
		_particleSystems.Add(_sporeParticleSystem);
		_sporeDamageArea = new CursedMushroomSporeDamageArea(_level, Position, base.DefaultTeam, this, base.Damage);
		_saplingObjectSpec = objectSpec;
		_minionContainer = new MinionContainer(16, _level);
	}

	public override void InitializeMob()
	{
		_minionContainer.Initialize();
		base.InitializeMob();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_isRunningDeathScript)
		{
			if (_hitSfxEmissionTimer > 0f)
			{
				_hitSfxEmissionTimer -= delta;
			}
			UpdateShake(delta);
			UpdateMinions(delta);
		}
		base.Update(delta);
	}

	private void UpdateShake(float delta)
	{
		if (!_isShaking)
		{
			return;
		}
		_hasBeenTouched = true;
		_shakeMagnitude -= delta * 10f;
		if (_shakeMagnitude <= 0f)
		{
			_shakeMagnitude = 0f;
			_isShaking = false;
			ChangeAnimation(new AnimationSpec[2]
			{
				new AnimationSpec
				{
					Start = 0,
					InitialIndex = _animationIndex,
					Length = 7,
					Speed = 0.125f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 0,
					Type = EAnimationType.None
				}
			});
			_sporeDamageArea.Life = 1f;
		}
		else
		{
			float emissionMultiplier = 1f * _shakeMagnitude;
			_sporeParticleSystem.AddParticles(new Vector2(Bbox.Center.X, Bbox.Top + 4), emissionMultiplier);
			_animationSpeed += 0.1f * delta;
			if (_animationSpeed > 0.125f)
			{
				_animationSpeed = 0.125f;
			}
		}
	}

	private void UpdateMinions(float delta)
	{
		if (_sporeDamageArea != null && _sporeDamageArea.Life > 0f && _hasBeenTouched)
		{
			_sproutTimer += delta;
			if (_sproutTimer > 0.25f)
			{
				_sproutTimer -= 0.25f;
				SproutMinion(_level.GetPlayerPosition().X < Position.X);
			}
		}
		_minionContainer.Update(delta);
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		_lastDamagePoint = where;
		_lastDamageVector = velocity;
		if (flag)
		{
			IncrementShaking();
		}
		return flag;
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		IncrementShaking();
		target.GiveStatusEffect(EStatusEffectType.Poison, 0);
		base.AfterDealingTouchDamage(target, effectPosition);
	}

	private void IncrementShaking()
	{
		if (!_isShaking)
		{
			_isShaking = true;
			ChangeAnimation(0, 7, 0.03f, EAnimationType.Cycle);
		}
		if (_hitSfxEmissionTimer <= 0f)
		{
			PlayCue(ESFX.EnemyMushroomTowerEmit, Position);
			_hitSfxEmissionTimer = 0.1f;
		}
		_shakeMagnitude += 5f;
		if (_shakeMagnitude > 20f)
		{
			_shakeMagnitude = 20f;
		}
		_animationSpeed = 0.03f;
		_sporeDamageArea.Refresh((int)(_shakeMagnitude * 4f) + 56, base.IsFrozen);
		_level.RequestAddObject(_sporeDamageArea);
	}

	private void SproutMinion(bool isFacingLeft)
	{
		if (!_minionContainer.IsFull)
		{
			Point position = Position;
			position = new Point(position.X + _level.NextRandomInt(-12, 12), position.Y);
			CursedMushroomMinion cursedMushroomMinion = new CursedMushroomMinion(position, _level, _sprite, -1, _saplingObjectSpec, base.MaxHP, base.Damage);
			cursedMushroomMinion.IsFacingLeft = isFacingLeft;
			CursedMushroomMinion newMinion = cursedMushroomMinion;
			_minionContainer.AddMinion(newMinion);
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			base.IsSolidWhenFrozen = false;
			_level.PlayCue(ESFX.EnemyMushroomTowerDeath, Position);
			SetCharacterSequenceByName("Destructable");
			UpdateCharacterSequences(0f);
			UpdateAppendages(0f);
			DebrisEvent.CreateFromObject(this, _lastDamageVector, _lastDamagePoint, _sprite, DebrisEvent.EDebrisDeathType.Dust);
			_appendages.Clear();
			if (_sporeDamageArea.Life > 0.5f)
			{
				_sporeDamageArea.Life = 0.5f;
			}
			DropLoot();
		}
		else if (_sporeParticleSystem.AreParticlesDone)
		{
			_sporeDamageArea.Kill();
			RemoveInstance();
		}
		else
		{
			UpdateParticleSystems(delta);
		}
		_deathScriptTimer += delta;
	}
}
