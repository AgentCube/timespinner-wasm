using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CursedAnemoneSpine : Projectile
{
	public CursedAnemoneSpine(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 4, inPosition.Y - 4, 8, 3);
		_bboxOffset = new Point(0, 1);
		_power = (int)Math.Ceiling(1.15f * (float)baseDamage);
		_force = 0;
		_life = 3f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesDieOnTiles = false;
		base.DoesCollideWithTiles = false;
		base.DoesDieOnImpact = false;
		ChangeAnimation(24);
	}
}
