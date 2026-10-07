using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal sealed class IceOrbSpellSnowProjectile : LunaisBaseProjectile
{
	private const int MinimumSkipDistanceSquared = 1024;

	private const int BouncesBeforeDying = 3;

	private const int BounceBoostX = 50;

	private const float MaxLife = 2f;

	private readonly int _spellDamage;

	private readonly LunaisSpell _parentSpell;

	private bool _hasDamagedSomething;

	private int _bounceCount;

	private Point _lastGroundContactPoint;

	public IceOrbSpellSnowProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, int baseDamage, LunaisSpell parentSpell)
		: base(inLevel, inPosition, iV, inSide, -1, parentSpell)
	{
		_parentSpell = parentSpell;
		_spellDamage = baseDamage;
		_sprite = _level.GCM.SpOrbMeleeIce;
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 8, 8);
		SnapBboxToPosition();
		ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 0,
				Length = 3,
				Speed = 0.066f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 3,
				Length = 2,
				Speed = 0.066f,
				Type = EAnimationType.Cycle
			}
		});
		_power = (int)Math.Ceiling((float)_spellDamage / 2f);
		_force = 1;
		_life = 2f;
		_damageElement = EDamageElement.Ice;
		_velocity = iV;
		_airDragFactor = 0.025f;
		_isAffectedByGravity = true;
		_isAffectedByFriction = true;
		_isFlying = false;
		base.DoesCollideWithTiles = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithFloors = true;
		_doesDieOnTiles = true;
		_doesRotateBasedOnVelocity = false;
		base.DoesDieOnImpact = false;
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		bool flag = true;
		if (_bounceCount > 0)
		{
			Point point = contactPoint.Subtract(_lastGroundContactPoint);
			int num = point.X * point.X + point.Y * point.Y;
			flag = num >= 1024;
		}
		if (flag)
		{
			GrowIcicle(contactPoint);
		}
		if (flag && _bounceCount < 3)
		{
			_bounceCount++;
			_velocity.Y = 0f - _velocity.Y;
			_velocity.X *= 50f;
		}
		else
		{
			SilentKill();
		}
		_lastGroundContactPoint = contactPoint;
	}

	public override bool DetermineDamage(Alive target, Rectangle collisionRectangle)
	{
		bool flag = false;
		if (!_hasDamagedSomething)
		{
			flag = base.DetermineDamage(target, collisionRectangle);
			if (flag)
			{
				_hasDamagedSomething = true;
				base.CanDamageEnemies = false;
				base.DoesDieOnImpact = true;
			}
		}
		return flag;
	}

	private void GrowIcicle(Point position)
	{
		IceOrbSpellSpikeDamageArea newProjectile = new IceOrbSpellSpikeDamageArea(_level, position, ETeamSide.Heroes, _spellDamage, _parentSpell);
		_level.AddProjectile(newProjectile);
	}
}
