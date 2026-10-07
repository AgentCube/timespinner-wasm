using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorBladeMeleeDamageArea : DamageArea
{
	private const int DamageSize = 80;

	private const float TransitionTime = 0.25f;

	private const float SleepTime = 0.125f;

	private const float MaxLife = 2.65f;

	public EmperorBladeMeleeDamageArea(Level inLevel, Point inPosition, Mobile inAnchor, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, inAnchor)
	{
		base.AnchorOffset = new Point(0, -6);
		Bbox = new Rectangle(Position.X, Position.Y, 80, 80);
		SnapBboxToPosition();
		_doesDrawSpriteAndAppendages = false;
		base.DormantTimer = 0.125f;
		base.Power = (int)Math.Ceiling((float)baseDamage * 1.25f);
		_force = 1;
		_life = 2.65f;
		base.DamageTimeoutTime = 0.2f;
		_damageElement = EDamageElement.Sharp;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		base.DoesKnockBack = true;
	}
}
