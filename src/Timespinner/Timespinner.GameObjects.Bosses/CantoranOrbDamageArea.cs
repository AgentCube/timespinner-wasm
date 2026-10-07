using System;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses;

internal sealed class CantoranOrbDamageArea : DamageArea
{
	internal CantoranOrbDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, int baseDamage)
		: base(inLevel, inPosition, inSide, -1, inAnchor)
	{
		base.Power = (int)Math.Ceiling(1.15f * (float)baseDamage);
		base.DamageDimensions = new Point(24, 24);
		base.Life = 100f;
		base.DamageTimeoutTime = 0.1f;
		base.HasInfiniteLife = true;
		_canDamageThings = false;
		base.DoesKnockBack = true;
		base.AnchorOffset = new Point(0, 0);
	}
}
