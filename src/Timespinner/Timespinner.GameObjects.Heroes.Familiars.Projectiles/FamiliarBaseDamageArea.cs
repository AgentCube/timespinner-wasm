using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars.Projectiles;

internal class FamiliarBaseDamageArea : DamageArea
{
	private readonly FamiliarBase _parentFamiliar;

	internal bool IsMeleeAttack { get; set; }

	public FamiliarBaseDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, FamiliarBase parentFamiliar)
		: base(inLevel, inPosition, inSide, -1, inAnchor)
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
