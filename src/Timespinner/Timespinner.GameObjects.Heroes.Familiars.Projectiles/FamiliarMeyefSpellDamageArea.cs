using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars.Projectiles;

internal sealed class FamiliarMeyefSpellDamageArea : FamiliarBaseDamageArea
{
	private const int FireballStorageSize = 64;

	private const float MaxLife = 5f;

	private readonly MeyefFireballUnit[] _fireballs = new MeyefFireballUnit[64];

	private int _fireballCreationCount;

	internal bool IsFinished { get; private set; }

	public FamiliarMeyefSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseOrbDamage, FamiliarBase parentFamiliar, SpriteSheet sprite)
		: base(inLevel, inPosition, inSide, null, parentFamiliar)
	{
		_sprite = sprite;
		_power = baseOrbDamage;
		_force = 0;
		_life = 5f;
		_timeToFade = 0f;
		_damageElement = EDamageElement.Fire;
		base.DamageTimeoutTime = 0.1f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_animationSpeed = 0f;
		_isFlying = true;
		base.DoesDieToEnemyProjectiles = false;
		base.DoesCollideWithTiles = true;
		_doesDieOnTiles = true;
		_doesCollideWithFloors = true;
		_doesCollideWithWalls = false;
		_doesCollideWithCeilings = true;
		_isIgnoringPlatform = false;
		_doesUseAppendageCollision = true;
		_doesDieOutsideOfVisibleArea = false;
		_doesDrawBaseSprite = false;
		_doAppendagesInheritDrawColor = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !IsFinished && _fireballCreationCount > 0)
		{
			for (int i = 0; i < _fireballCreationCount; i++)
			{
				MeyefFireballUnit meyefFireballUnit = _fireballs[i];
				if (meyefFireballUnit.IsFinished && !meyefFireballUnit.HasBeenRemoved)
				{
					base.Appendages.Remove(meyefFireballUnit);
					meyefFireballUnit.HasBeenRemoved = true;
				}
			}
		}
		base.Update(delta);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.PlayCue(ESFX.LunaisOrbImpactBurn, intersectionCenter);
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			TeamSide = _teamSide,
			AnimationSpeed = 0.03f,
			AnimationStart = 35,
			AnimationLength = 4,
			IsFacingLeft = !target.IsFacingLeft
		});
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.Appendages.Clear();
		base.SilentKill();
	}

	internal void End()
	{
		float num = 0f;
		for (int i = 0; i < _fireballCreationCount; i++)
		{
			MeyefFireballUnit meyefFireballUnit = _fireballs[i];
			if (!meyefFireballUnit.IsFinished && meyefFireballUnit.Life > num)
			{
				num = meyefFireballUnit.Life;
			}
		}
		_life = num;
	}

	internal void Reset(Point position, int power)
	{
		Position = position;
		SnapBboxToPosition();
		base.ID = -1;
		_power = power;
		_isFading = false;
		IsFinished = false;
		base.CanDamageEnemies = true;
		_life = 5f;
		base.Appendages.Clear();
	}

	internal void EmitFireball(Point position, Vector2 iV)
	{
		MeyefFireballUnit meyefFireballUnit = null;
		if (_fireballCreationCount < 64)
		{
			meyefFireballUnit = new MeyefFireballUnit(this, _level, _sprite);
			_fireballs[_fireballCreationCount] = meyefFireballUnit;
			_fireballCreationCount++;
		}
		else
		{
			for (int i = 0; i < 64; i++)
			{
				if (_fireballs[i].IsFinished)
				{
					meyefFireballUnit = _fireballs[i];
					break;
				}
			}
		}
		if (meyefFireballUnit != null)
		{
			meyefFireballUnit.Reset(position, iV);
			base.Appendages.Add(meyefFireballUnit);
		}
	}

	internal void ChangeSprite(SpriteSheet sprite)
	{
		_sprite = sprite;
		MeyefFireballUnit[] fireballs = _fireballs;
		for (int i = 0; i < fireballs.Length; i++)
		{
			fireballs[i]?.ChangeSprite(sprite);
		}
	}
}
