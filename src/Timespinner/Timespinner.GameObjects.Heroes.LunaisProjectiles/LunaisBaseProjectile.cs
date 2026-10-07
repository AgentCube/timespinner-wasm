using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal class LunaisBaseProjectile : Projectile
{
	private readonly LunaisOrbAbility _parentOrb;

	public LunaisBaseProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, int inID, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, iV, inSide, inID)
	{
		_parentOrb = parentOrb;
	}

	public LunaisBaseProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, float dormantTime, int inID, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, iV, inSide, dormantTime, inID)
	{
		_parentOrb = parentOrb;
	}

	public override bool DetermineDamage(Alive target, Rectangle collisionRectangle)
	{
		bool flag = base.DetermineDamage(target, collisionRectangle);
		if (_parentOrb != null && flag && target != null && target.DefaultTeam == ETeamSide.Enemies)
		{
			_parentOrb.OnSuccessfulEnemyHit(target);
		}
		return flag;
	}
}
