using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Events.LevelEffects;

internal class LakeVacuumLevelEffect : LevelEffect
{
	private const float PlayerCheckIntervalTime = 0.25f;

	private float _playerCheckTimer;

	public LakeVacuumLevelEffect(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_isAffectedByTime = false;
	}

	public override void Update(float delta)
	{
		_playerCheckTimer += delta;
		if (_playerCheckTimer >= 0.25f)
		{
			_playerCheckTimer -= 0.25f;
			if (!_level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.AirMask))
			{
				_level.MainHero?.GiveStatusEffect(EStatusEffectType.Suffocate, 1);
			}
		}
		base.Update(delta);
	}
}
