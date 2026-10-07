using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class MoonOrbMeleeDamageArea : LunaisBaseOrbDamageArea
{
	private const float MaxLife = 100f;

	private readonly LunaisMoonOrb _moonOrb;

	private int _baseDamage;

	private int _hitCount;

	public MoonOrbMeleeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, LunaisMoonOrb inMoonOrb, int inDamage, LunaisOrb parentOrb)
		: base(inLevel, inPosition, inSide, -1, null, parentOrb)
	{
		_moonOrb = inMoonOrb;
		_sprite = null;
		_doesDrawSpriteAndAppendages = false;
		Rectangle bbox = _moonOrb.Bbox;
		_damageDimensions = new Point(bbox.Width, bbox.Height);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		SnapBboxToPosition();
		CalculateDamage(inDamage);
		_force = 1;
		_life = 100f;
		base.DamageTimeoutTime = 0.1f;
		_damageElement = EDamageElement.Blunt;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
	}

	public override void Update(float delta)
	{
		Bbox = _moonOrb.OuterBbox;
		Position = _moonOrb.OuterBbox.Center;
		base.Update(delta);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(EBattleAnimationType.MediumHit, intersectionCenter, _teamSide, _moonOrb.Position.X < target.Position.X, doesPlaySFX: false);
		_level.PlayCue(ESFX.LunaisOrbImpact, intersectionCenter);
	}

	private void CalculateDamage(int damage)
	{
		_power = damage;
		_baseDamage = _power;
	}

	internal void Reset(int damage)
	{
		CalculateDamage(damage);
		base.ID = -1;
		_life = 100f;
		_hitCount = 0;
		Update(0f);
	}

	public override bool DetermineDamage(Alive target, Rectangle collisionRectangle)
	{
		if (_hitCount > 0)
		{
			int num = 1;
			for (int i = 0; i < _hitCount; i++)
			{
				num *= 2;
			}
			if (num > 0)
			{
				base.Power = (int)Math.Ceiling((float)_baseDamage / (float)num);
			}
			else
			{
				base.Power = 1f;
			}
		}
		if (base.Power < 0f)
		{
			base.Power = 0f;
		}
		bool flag = base.DetermineDamage(target, collisionRectangle);
		if (flag)
		{
			_hitCount++;
			if (_hitCount > 16)
			{
				_hitCount = 16;
			}
		}
		return flag;
	}
}
