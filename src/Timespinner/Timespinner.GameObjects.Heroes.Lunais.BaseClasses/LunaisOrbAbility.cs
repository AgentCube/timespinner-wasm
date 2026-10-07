using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

internal class LunaisOrbAbility : Animate
{
	private readonly HashSet<int> _hitEnemyRegistry = new HashSet<int>();

	internal EOrbSlot SlotType { get; set; }

	internal HashSet<int> HitEnemyRegistry => _hitEnemyRegistry;

	public LunaisOrbAbility(Point inPosition, Level inLevel, int inID)
		: base(inPosition, inLevel, inID)
	{
		_isAffectedByGravity = false;
		_isFlying = true;
	}

	private void RememberHitEnemy(int enemyID)
	{
		if (!_hitEnemyRegistry.Contains(enemyID))
		{
			_hitEnemyRegistry.Add(enemyID);
		}
	}

	internal virtual void OnEnemyContact(Alive enemy, LunaisBaseOrbDamageArea damageArea, Rectangle contactBbox)
	{
	}

	internal virtual void OnSuccessfulEnemyHit(Alive enemy)
	{
		if (enemy != null)
		{
			RememberHitEnemy(enemy.ID);
		}
	}
}
