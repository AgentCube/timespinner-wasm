using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars.Projectiles;

internal class FamiliarBaseProjectile : Projectile
{
	private readonly FamiliarBase _parentFamiliar;

	internal bool IsMeleeAttack { get; set; }

	public FamiliarBaseProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, FamiliarBase parentFamiliar)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_parentFamiliar = parentFamiliar;
	}

	public FamiliarBaseProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, float dormantTime, FamiliarBase parentFamiliar)
		: base(inLevel, inPosition, iV, inSide, dormantTime, -1)
	{
		_parentFamiliar = parentFamiliar;
	}

	public override bool DetermineDamage(Alive target, Rectangle collisionRectangle)
	{
		base.DamageMultiplier = 1f;
		bool flag = base.DetermineDamage(target, collisionRectangle);
		if (_parentFamiliar != null && flag && target != null && target.DefaultTeam == ETeamSide.Enemies)
		{
			_parentFamiliar.OnSuccessfulEnemyHit(target, IsMeleeAttack);
		}
		return flag;
	}
}
