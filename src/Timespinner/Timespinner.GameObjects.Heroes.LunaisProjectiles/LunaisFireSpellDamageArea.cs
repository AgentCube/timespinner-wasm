using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class LunaisFireSpellDamageArea : LunaisBaseOrbDamageArea
{
	private const int FireballStorageSize = 80;

	private const float MaxLife = 5f;

	private const float EmissionAnimationTimeToFade = 0.1f;

	private static readonly Color InvisibleColor = new Color(0, 0, 0, 0);

	private readonly BattleAnimation _emissionAnimation;

	private readonly LunaisFireballUnit[] _fireballs = new LunaisFireballUnit[80];

	private bool _isEmissionAnimationFadingIn;

	private int _fireballCreationCount;

	private float _emissionAnimationFadeTimer;

	internal bool IsFinished { get; private set; }

	public LunaisFireSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseOrbDamage, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, inSide, -1, null, parentOrb)
	{
		_sprite = _level.GCM.SpOrbMeleeFire;
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
		_emissionAnimation = new BattleAnimation(_sprite, inPosition.Add(IsFacingLeft ? 1 : 0, 0), _level)
		{
			AnimationStart = 0,
			AnimationLength = 4,
			DoesRepeat = true,
			DrawColor = InvisibleColor,
			IsFacingLeft = IsFacingLeft
		};
		_isEmissionAnimationFadingIn = true;
		_emissionAnimationFadeTimer = 0.1f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !IsFinished && _fireballCreationCount > 0)
		{
			for (int i = 0; i < _fireballCreationCount; i++)
			{
				LunaisFireballUnit lunaisFireballUnit = _fireballs[i];
				if (lunaisFireballUnit.IsFinished && !lunaisFireballUnit.HasBeenRemoved)
				{
					base.Appendages.Remove(lunaisFireballUnit);
					lunaisFireballUnit.HasBeenRemoved = true;
				}
			}
			if (_emissionAnimationFadeTimer > 0f)
			{
				_emissionAnimationFadeTimer -= delta;
				if (_emissionAnimationFadeTimer <= 0f)
				{
					_emissionAnimation.DrawColor = (_isEmissionAnimationFadingIn ? Color.White : InvisibleColor);
				}
				else
				{
					float num = _emissionAnimationFadeTimer / 0.1f;
					if (_isEmissionAnimationFadingIn)
					{
						num = 1f - num;
					}
					_emissionAnimation.DrawColor = Color.White * num;
				}
			}
			_emissionAnimation.Update(delta);
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
			AnimationStart = 38,
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
			LunaisFireballUnit lunaisFireballUnit = _fireballs[i];
			if (!lunaisFireballUnit.IsFinished && lunaisFireballUnit.Life > num)
			{
				num = lunaisFireballUnit.Life;
			}
		}
		_isEmissionAnimationFadingIn = false;
		_emissionAnimationFadeTimer = 0.1f;
		_life = num;
	}

	internal void Reset(Point position, int power, bool isFacingLeft, LunaisObj parentLunais)
	{
		Position = position;
		SnapBboxToPosition();
		base.ID = -1;
		_power = power;
		_isFading = false;
		IsFinished = false;
		base.CanDamageEnemies = true;
		_life = 5f;
		_emissionAnimation.AnchorObject = parentLunais;
		_emissionAnimation.IsFacingLeft = isFacingLeft;
		_emissionAnimation.AnchorOffset = new Point(20 * ((!isFacingLeft) ? 1 : (-1)), -23);
		_isEmissionAnimationFadingIn = true;
		_emissionAnimationFadeTimer = 0.1f;
		base.Appendages.Clear();
	}

	internal void EmitFireball(Point position, Vector2 iV)
	{
		LunaisFireballUnit lunaisFireballUnit = null;
		if (_fireballCreationCount < 80)
		{
			lunaisFireballUnit = new LunaisFireballUnit(this, _level, _sprite);
			_fireballs[_fireballCreationCount] = lunaisFireballUnit;
			_fireballCreationCount++;
		}
		else
		{
			for (int i = 0; i < 80; i++)
			{
				if (_fireballs[i].IsFinished)
				{
					lunaisFireballUnit = _fireballs[i];
					break;
				}
			}
		}
		if (lunaisFireballUnit != null)
		{
			lunaisFireballUnit.Reset(position, iV);
			base.Appendages.Add(lunaisFireballUnit);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_emissionAnimation.Draw(spriteBatch);
		base.Draw(spriteBatch);
	}
}
