using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorFireMeleeDamageArea : DamageArea
{
	private const int DamageSize = 12;

	private const float MaxLife = 1.65f;

	public EmperorFireMeleeDamageArea(Level inLevel, Point inPosition, Mobile inAnchor, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, inAnchor)
	{
		base.AnchorOffset = new Point(0, -6);
		Bbox = new Rectangle(Position.X, Position.Y, 12, 12);
		SnapBboxToPosition();
		_doesDrawSpriteAndAppendages = false;
		base.Power = (int)Math.Ceiling((float)baseDamage * 1f);
		_force = 1;
		_life = 1.65f;
		base.DamageTimeoutTime = 0.2f;
		_damageElement = EDamageElement.Fire;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		base.DoesKnockBack = true;
	}
}
