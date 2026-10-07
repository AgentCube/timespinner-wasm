using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class IceOrbMeleeSnowProjectile : LunaisBaseProjectile
{
	private const float MaxLife = 2f;

	private readonly int _baseDamage;

	private readonly LunaisOrb _parentOrb;

	private bool _hasDamagedSomething;

	private bool _hasGrownAnIcicle;

	public IceOrbMeleeSnowProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, int inDamage, LunaisOrb parentOrb)
		: base(inLevel, inPosition, iV, inSide, -1, parentOrb)
	{
		_parentOrb = parentOrb;
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
		IsFacingLeft = _parentOrb.IsThrowingLeft;
		_baseDamage = inDamage;
		_power = (int)Math.Ceiling((float)_baseDamage * 0.5f);
		_force = 1;
		_life = 2f;
		_damageElement = EDamageElement.Ice;
		_velocity = iV;
		_airDragFactor = 0.05f;
		_isAffectedByGravity = true;
		_isAffectedByFriction = true;
		_isFlying = false;
		base.DoesCollideWithTiles = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithFloors = true;
		_doesDieOnTiles = true;
		_doesCollideWithWalls = false;
		_doesCollideWithCeilings = false;
		_doesRotateBasedOnVelocity = false;
		base.DoesDieOnImpact = false;
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		GrowIcicle(contactPoint);
		SilentKill();
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
		if (!_hasGrownAnIcicle)
		{
			_hasGrownAnIcicle = true;
			_level.AddAnimation(new BattleAnimation(_sprite, position.Add(0, -13), _level)
			{
				AnimationStart = 5,
				AnimationLength = 6,
				TeamSide = base.TeamSide
			});
			Point inPosition = new Point(position.X, position.Y + 4);
			_level.AddProjectile(new IceOrbIcicleDamageArea(_level, inPosition, _teamSide, _baseDamage, -1, _parentOrb));
		}
	}
}
