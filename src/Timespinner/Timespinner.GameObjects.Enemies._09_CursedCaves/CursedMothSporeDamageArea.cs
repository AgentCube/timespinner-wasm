using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Enemies._09_CursedCaves;

internal class CursedMothSporeDamageArea : DamageArea
{
	public CursedMothSporeDamageArea(Level inLevel, Point inPosition, int power)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		Bbox = new Rectangle(0, 0, 16, 16);
		_isAffectedByGravity = true;
		_gravityAcceleration = 50f;
		_maxFallSpeed = 30f;
		base.Power = power;
		base.Life = 1.2f;
		base.DoesCollideWithTiles = true;
		base.DoesDieOnImpact = true;
		_doesCollideWithCeilings = false;
		_doesCollideWithFloors = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithWalls = false;
	}

	public override bool DetermineDamage(Alive target, Rectangle collidingBbox)
	{
		bool flag = base.DetermineDamage(target, collidingBbox);
		if (flag)
		{
			_level.PlayCue(ESFX.EnemyMushroomTowerSporeHit, target.Position);
			target.GiveStatusEffect(EStatusEffectType.NeuroToxin, 0);
		}
		return flag;
	}
}
