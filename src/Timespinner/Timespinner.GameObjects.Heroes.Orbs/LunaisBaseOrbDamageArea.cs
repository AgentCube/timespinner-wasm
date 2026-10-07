using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal class LunaisBaseOrbDamageArea : DamageArea
{
	private readonly LunaisOrbAbility _parentOrb;

	public LunaisBaseOrbDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int inID, Mobile inAnchor, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, inSide, inID, inAnchor)
	{
		_parentOrb = parentOrb;
	}

	public override bool DetermineDamage(Alive target, Rectangle collisionRectangle)
	{
		base.DamageMultiplier = 1f;
		if (_parentOrb != null && !target.IsInvulnerable && base.DamageTimeoutTime >= 0f && !_damagedEnemiesDictionary.ContainsKey(target.ID))
		{
			_parentOrb.OnEnemyContact(target, this, collisionRectangle);
		}
		bool flag = base.DetermineDamage(target, collisionRectangle);
		if (_parentOrb != null && flag && target != null && target.DefaultTeam == ETeamSide.Enemies)
		{
			_parentOrb.OnSuccessfulEnemyHit(target);
		}
		return flag;
	}
}
