using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Saving;

internal class GameFeat
{
	private const int IconIndexStart = 192;

	private const string BasicFeatNameKey = "feat_name_{0}";

	private const string BasicFeatDescriptionKey = "feat_desc_{0}";

	internal const string BasicFeatSaveKey = "{0}";

	private const string EnemyNameKey = "Enemy_{0}_name";

	private const string BossFeatNameKey = "feat_name_Boss";

	private const string BossFeatProperDescriptionKey = "feat_desc_Boss_proper_noun";

	private const string BossFeatCommonDescriptionKey = "feat_desc_Boss_common_noun";

	internal const string SpecificFeatSaveKey = "{0}_{1}";

	private readonly bool _isBoss;

	private readonly EGameFeatType _type;

	private readonly EGameFeatIcon _icon;

	private readonly string _name;

	private readonly string _description;

	private readonly string _key;

	internal bool IsUnlocked { get; private set; }

	internal bool IsHidden { get; private set; }

	internal bool IsBoss => _isBoss;

	internal EGameFeatType Type => _type;

	internal EGameFeatIcon Icon => _icon;

	internal int ProgressValue { get; set; }

	internal string Name => _name;

	internal string Description => _description;

	internal string Key => _key;

	internal GameFeat(EGameFeatType type)
	{
		_type = type;
		_icon = GetIconFromType(_type);
		_name = Loc.Get($"feat_name_{type}");
		_description = Loc.Get($"feat_desc_{type}");
		_key = $"{type}";
		switch (_type)
		{
		case EGameFeatType.EndingC:
		case EGameFeatType.EndingD:
			IsHidden = true;
			break;
		}
	}

	internal GameFeat(EGameFeatType type, EBossType bossType)
	{
		_type = type;
		_icon = EGameFeatIcon.PlaceholderBoss;
		_isBoss = true;
		switch (bossType)
		{
		case EBossType.RoboKitty:
			_icon = EGameFeatIcon.BossRoboKitty;
			break;
		case EBossType.Varndagroth:
			_icon = EGameFeatIcon.BossVarndagroth;
			break;
		case EBossType.Bird:
			_icon = EGameFeatIcon.BossAzureQueen;
			break;
		case EBossType.Demon:
			_icon = EGameFeatIcon.BossGoldenIdol;
			break;
		case EBossType.Maw:
			_icon = EGameFeatIcon.BossMaw;
			break;
		case EBossType.Sorceress:
			_icon = EGameFeatIcon.BossAelana;
			break;
		case EBossType.Shapeshift:
			_icon = EGameFeatIcon.BossGenza;
			break;
		case EBossType.Emperor:
			_icon = EGameFeatIcon.BossEmperor;
			break;
		case EBossType.Sandman:
			_icon = EGameFeatIcon.BossSandman;
			break;
		case EBossType.Nightmare:
			_icon = EGameFeatIcon.BossNightmare;
			break;
		case EBossType.Raven:
			_icon = EGameFeatIcon.BossRavenlord;
			break;
		case EBossType.Xarion:
			_icon = EGameFeatIcon.BossXarion;
			break;
		case EBossType.Zel:
			_icon = EGameFeatIcon.BossZel;
			break;
		case EBossType.Cantoran:
			_icon = EGameFeatIcon.BossCantoran;
			break;
		}
		bool flag = false;
		switch (bossType)
		{
		case EBossType.Varndagroth:
		case EBossType.Maw:
		case EBossType.Sorceress:
		case EBossType.Shapeshift:
		case EBossType.Emperor:
		case EBossType.Nightmare:
		case EBossType.Xarion:
		case EBossType.Zel:
		case EBossType.Cantoran:
			flag = true;
			break;
		}
		EEnemyTileType enemyTypeFromBossType = BossClass.GetEnemyTypeFromBossType(bossType);
		string arg = Loc.Get($"Enemy_{enemyTypeFromBossType}_name");
		_name = string.Format(Loc.Get("feat_name_Boss"), arg);
		_description = string.Format(Loc.Get(flag ? "feat_desc_Boss_proper_noun" : "feat_desc_Boss_common_noun"), arg);
		_key = $"{type}_{bossType}";
		IsHidden = true;
	}

	internal void Unlock()
	{
		IsHidden = false;
		IsUnlocked = true;
	}

	private static EGameFeatIcon GetIconFromType(EGameFeatType type)
	{
		EGameFeatIcon result = EGameFeatIcon.PlaceholderBoss;
		switch (type)
		{
		case EGameFeatType.AllFeats:
			result = EGameFeatIcon.AllFeats;
			break;
		case EGameFeatType.FinishGame:
			result = EGameFeatIcon.FinishGame;
			break;
		case EGameFeatType.EndingC:
			result = EGameFeatIcon.EndingC;
			break;
		case EGameFeatType.EndingD:
			result = EGameFeatIcon.EndingD;
			break;
		case EGameFeatType.FinishGameHard:
			result = EGameFeatIcon.FinishGameHard;
			break;
		case EGameFeatType.GetOrb:
			result = EGameFeatIcon.GetOrb;
			break;
		case EGameFeatType.GetAllOrbs:
			result = EGameFeatIcon.GetAllOrbs;
			break;
		case EGameFeatType.MakeSpell:
			result = EGameFeatIcon.MakeSpell;
			break;
		case EGameFeatType.MakeAllSpells:
			result = EGameFeatIcon.MakeAllSpells;
			break;
		case EGameFeatType.MakePassive:
			result = EGameFeatIcon.MakePassive;
			break;
		case EGameFeatType.MakeAllPassives:
			result = EGameFeatIcon.MakeAllPassives;
			break;
		case EGameFeatType.GetFamiliar:
			result = EGameFeatIcon.GetFamiliar;
			break;
		case EGameFeatType.GetAllFamiliars:
			result = EGameFeatIcon.GetAllFamiliars;
			break;
		case EGameFeatType.FinishQuest:
			result = EGameFeatIcon.FinishQuest;
			break;
		case EGameFeatType.FinishQuestLine:
			result = EGameFeatIcon.FinishQuestLine;
			break;
		case EGameFeatType.FinishAllQuests:
			result = EGameFeatIcon.FinishAllQuests;
			break;
		case EGameFeatType.Boss:
			result = EGameFeatIcon.PlaceholderBoss;
			break;
		case EGameFeatType.StealOrb:
			result = EGameFeatIcon.StealOrb;
			break;
		case EGameFeatType.GetAllMemories:
			result = EGameFeatIcon.GetAllMemories;
			break;
		case EGameFeatType.GetAllLetters:
			result = EGameFeatIcon.GetAllLetters;
			break;
		case EGameFeatType.GetAllFiles:
			result = EGameFeatIcon.GetAllFiles;
			break;
		case EGameFeatType.GetAllJournals:
			result = EGameFeatIcon.GetAllJournals;
			break;
		case EGameFeatType.MapCompletion:
			result = EGameFeatIcon.MapCompletion;
			break;
		case EGameFeatType.PerfectBoss:
			result = EGameFeatIcon.PerfectBoss;
			break;
		}
		return result;
	}

	internal int GetIconIndex()
	{
		return (int)(192 + _icon);
	}

	public void InitializeFromSave(int saveValue)
	{
		IsUnlocked = saveValue > 0;
		ProgressValue = saveValue;
	}
}
