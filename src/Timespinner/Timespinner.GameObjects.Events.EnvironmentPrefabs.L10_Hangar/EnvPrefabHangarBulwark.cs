using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L10_Hangar;

internal sealed class EnvPrefabHangarBulwark : EnvironmentPrefabBase
{
	private const int GemCount = 3;

	private readonly BulwarkGemAppendage[] _gemAppendages = new BulwarkGemAppendage[3];

	public EnvPrefabHangarBulwark(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscHangar2;
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		IsFacingLeft = true;
		_bboxOffset = Point.Zero;
		Bbox = new Rectangle(0, 0, 16, 16);
		_doAppendagesMatchImageFacing = true;
		_isSolid = false;
		_isAffectedByGravity = false;
		base.DrawPlane = EDrawPlane.Back;
		bool saveBool = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Demon));
		bool saveBool2 = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Maw));
		bool saveBool3 = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Sorceress));
		for (int i = 0; i < 3; i++)
		{
			EBulwarkGemType eBulwarkGemType = (EBulwarkGemType)i;
			bool isDead = false;
			switch (eBulwarkGemType)
			{
			case EBulwarkGemType.Plasma:
				isDead = saveBool3;
				break;
			case EBulwarkGemType.Chaos:
				isDead = saveBool;
				break;
			case EBulwarkGemType.Blood:
				isDead = saveBool2;
				break;
			}
			BulwarkGemAppendage bulwarkGemAppendage = new BulwarkGemAppendage(this, _level, _sprite, eBulwarkGemType, isDead);
			_gemAppendages[i] = bulwarkGemAppendage;
			_appendages.Add(bulwarkGemAppendage);
		}
		CreateCue(ESFX.AmbientLazerLoop, Position, isLooped: true)?.PlayWhenInRange();
	}
}
