using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars;

internal class FamiliarManager
{
	private const int MaxFamiliar = 6;

	private const int Anim_MeyefMewStartStart = 51;

	private const int Anim_MeyefMewStartLength = 3;

	private const int Anim_MeyefMewEndStart = 54;

	private const int Anim_MeyefMewEndLength = 2;

	private const int FlyInFrontOfPlayerOffsetX = 40;

	private const int FlyInFrontOfPlayerOffsetY = -16;

	private const float Anim_MeyefMewSpeed = 0.1f;

	private const float Anim_MeyefMewLingerSpeed = 0.1f;

	private readonly LunaisObj _parentObject;

	private readonly Level _level;

	private bool _isMeyefAltSkinActive;

	private bool _isCrowAltSkinActive;

	private EInventoryFamiliarType _equippedFamiliarType;

	private FamiliarBase _equippedFamiliar;

	private FamiliarBase _cutsceneSwappedFamiliar;

	internal EInventoryFamiliarType EquippedFamiliarType => _equippedFamiliarType;

	internal FamiliarBase EquippedFamiliar => _equippedFamiliar;

	internal bool IsFamiliarControlledByAI
	{
		get
		{
			if (_equippedFamiliar != null)
			{
				return !_equippedFamiliar.IsPlayable;
			}
			return false;
		}
	}

	public FamiliarManager(LunaisObj parentObject)
	{
		_parentObject = parentObject;
		_level = _parentObject.Level;
	}

	public void ChangeFamiliar(EInventoryFamiliarType newFamiliar)
	{
		Point inPosition = ((_equippedFamiliar == null) ? _parentObject.Bbox.Center : _equippedFamiliar.Position);
		bool flag = false;
		PlayerIndex playerIndex = PlayerIndex.Two;
		if (_equippedFamiliar != null)
		{
			flag = _equippedFamiliar.IsPlayable;
			playerIndex = _equippedFamiliar.PlayerIndex;
			_equippedFamiliar.Unequip();
		}
		_equippedFamiliarType = newFamiliar;
		_equippedFamiliar = FamiliarBase.FromFamiliarType(inPosition, _parentObject, _equippedFamiliarType, SwitchFamiliar);
		if (_equippedFamiliar != null)
		{
			if (flag)
			{
				_equippedFamiliar.SetIsPlayable(isPlayable: true, isSwitching: true);
				_equippedFamiliar.SetPlayerIndex(playerIndex);
			}
			_level.RequestAddObject(_equippedFamiliar);
		}
	}

	public void RefreshStats(GameSave inSave)
	{
		if (inSave.Inventory.EquippedFamiliar != _equippedFamiliarType)
		{
			ChangeFamiliar(inSave.Inventory.EquippedFamiliar);
		}
		bool flag = inSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.FamiliarAltCrow);
		bool flag2 = inSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.FamiliarAltMeyef);
		if (flag != _isCrowAltSkinActive && _equippedFamiliarType == EInventoryFamiliarType.MerchantCrow && EquippedFamiliar != null)
		{
			EquippedFamiliar.ChangeSkin(flag);
		}
		if (flag2 != _isMeyefAltSkinActive && _equippedFamiliarType == EInventoryFamiliarType.Meyef && EquippedFamiliar != null)
		{
			EquippedFamiliar.ChangeSkin(flag2);
		}
		_isCrowAltSkinActive = flag;
		_isMeyefAltSkinActive = flag2;
	}

	public void ChangeRoom()
	{
		if (_equippedFamiliar != null)
		{
			_equippedFamiliar.ChangeRoom();
		}
	}

	public void GiveExperience(int enemyID)
	{
		if (_equippedFamiliar != null && _equippedFamiliar.GiveExperience(enemyID))
		{
			_level.AddAnimation(new SparkleTextAnimation(Loc.Get("LevelUpFanfare"), _level.GCM.SpEffectsMedium, _equippedFamiliar.Bbox.Center, _level, Color.LightBlue));
		}
	}

	internal void AddScriptAction(ScriptAction script)
	{
		if (script is FamiliarScript familiarScript)
		{
			Vector4 arguments = script.Arguments;
			switch (familiarScript.FamiliarScriptType)
			{
			case FamiliarScript.EFamiliarScriptType.Summon:
				SummonMeyef(script.Arguments.X > 0f);
				break;
			case FamiliarScript.EFamiliarScriptType.Dismiss:
				HideMeyef(script.Arguments.X > 0f, script.Arguments.Y > 0f);
				break;
			case FamiliarScript.EFamiliarScriptType.SitStill:
				if (_equippedFamiliar != null)
				{
					_equippedFamiliar.CutsceneSitStill();
				}
				break;
			case FamiliarScript.EFamiliarScriptType.ResumeAI:
				if (_equippedFamiliar != null)
				{
					_equippedFamiliar.CutsceneResumeAI();
				}
				break;
			case FamiliarScript.EFamiliarScriptType.CutsceneFlyTo:
				if (_equippedFamiliar != null)
				{
					_equippedFamiliar.CutsceneFlyTo(new Point((int)arguments.X, (int)arguments.Y), script.ActionTimer, (int)arguments.Z, arguments.W > 0f);
				}
				break;
			case FamiliarScript.EFamiliarScriptType.CutsceneFlyAround:
				if (_equippedFamiliar != null)
				{
					_equippedFamiliar.CutsceneFlyAround(new Point((int)arguments.X, (int)arguments.Y), script.ActionTimer, script.Arguments.Z);
				}
				break;
			case FamiliarScript.EFamiliarScriptType.CutsceneFlyInFrontOfPlayer:
				if (_equippedFamiliar != null)
				{
					bool isFacingLeft = _parentObject.IsFacingLeft;
					Point position = _parentObject.Position;
					Point targetPoint = new Point(position.X + (isFacingLeft ? (-40) : 40), position.Y + -16);
					_equippedFamiliar.CutsceneFlyTo(targetPoint, script.ActionTimer, isFacingLeft ? 1 : (-1), isCosineFlight: false);
				}
				break;
			case FamiliarScript.EFamiliarScriptType.MeyefMew:
				if (EquippedFamiliar != null && EquippedFamiliar.FamiliarType == EInventoryFamiliarType.Meyef)
				{
					AnimationSpec animationSpec = new AnimationSpec();
					animationSpec.Start = 51;
					animationSpec.Length = 3;
					animationSpec.Speed = 0.1f;
					animationSpec.Type = EAnimationType.Once;
					AnimationSpec item = animationSpec;
					AnimationSpec animationSpec2 = new AnimationSpec();
					animationSpec2.Start = 53;
					animationSpec2.Length = 1;
					animationSpec2.Speed = 0.1f;
					animationSpec2.Type = EAnimationType.Once;
					AnimationSpec item2 = animationSpec2;
					AnimationSpec animationSpec3 = new AnimationSpec();
					animationSpec3.Start = 54;
					animationSpec3.Length = 2;
					animationSpec3.Speed = 0.1f;
					animationSpec3.Type = EAnimationType.Once;
					AnimationSpec item3 = animationSpec3;
					AnimationSpec animationSpec4 = new AnimationSpec();
					animationSpec4.Start = 0;
					animationSpec4.Length = 5;
					animationSpec4.Speed = 0.1f;
					animationSpec4.Type = EAnimationType.Cycle;
					AnimationSpec item4 = animationSpec4;
					AnimationSpecCollection animationSpecCollection = new AnimationSpecCollection();
					animationSpecCollection.Collection.Add(item);
					animationSpecCollection.Collection.Add(item2);
					animationSpecCollection.Collection.Add(item3);
					animationSpecCollection.Collection.Add(item4);
					EquippedFamiliar.ChangeAnimation(animationSpecCollection);
				}
				break;
			case FamiliarScript.EFamiliarScriptType.Land:
				break;
			}
		}
		else if (EquippedFamiliar != null)
		{
			EquippedFamiliar.AddScriptAction(script);
		}
	}

	internal void SummonMeyef(bool isSilent)
	{
		Point inPosition = ((_equippedFamiliar == null) ? _parentObject.Bbox.Center : _equippedFamiliar.Position);
		if (!_level.GameSave.Inventory.FamiliarInventory.Inventory.ContainsKey(1))
		{
			_level.GameSave.GiveFamiliar(EInventoryFamiliarType.Meyef);
		}
		if (_equippedFamiliarType == EInventoryFamiliarType.None)
		{
			_equippedFamiliar = FamiliarBase.FromFamiliarType(inPosition, _parentObject, EInventoryFamiliarType.Meyef, SwitchFamiliar);
			if (_equippedFamiliar != null)
			{
				_level.AddFamiliar(_equippedFamiliar);
			}
		}
		else if (_equippedFamiliarType != EInventoryFamiliarType.Meyef)
		{
			if (!isSilent)
			{
				AddFamiliarPoofAnimation();
			}
			_cutsceneSwappedFamiliar = _equippedFamiliar;
			_level.RequestRemoveObject(_equippedFamiliar);
			_equippedFamiliar = FamiliarBase.FromFamiliarType(inPosition, _parentObject, EInventoryFamiliarType.Meyef, SwitchFamiliar);
			if (_equippedFamiliar != null)
			{
				_level.AddFamiliar(_equippedFamiliar);
			}
		}
	}

	internal void HideMeyef(bool shouldKeepMeyef, bool isInvisible)
	{
		if (_equippedFamiliarType == EInventoryFamiliarType.None)
		{
			if (!shouldKeepMeyef)
			{
				if (_equippedFamiliar != null)
				{
					if (!isInvisible)
					{
						AddFamiliarPoofAnimation();
					}
					_level.RequestRemoveObject(_equippedFamiliar);
					_equippedFamiliar = null;
				}
			}
			else
			{
				_equippedFamiliarType = EInventoryFamiliarType.Meyef;
				if (_equippedFamiliar != null)
				{
					_equippedFamiliar.CutsceneResumeAI();
				}
			}
		}
		else if (_cutsceneSwappedFamiliar != null)
		{
			if (!isInvisible)
			{
				AddFamiliarPoofAnimation();
			}
			_level.RequestRemoveObject(_equippedFamiliar);
			_cutsceneSwappedFamiliar.MatchPreviousFamiliarPosition(_equippedFamiliar);
			_equippedFamiliar = _cutsceneSwappedFamiliar;
			_level.RequestAddObject(_equippedFamiliar);
			_cutsceneSwappedFamiliar = null;
		}
		else if (_equippedFamiliar != null)
		{
			_equippedFamiliar.CutsceneResumeAI();
		}
	}

	private void AddFamiliarPoofAnimation()
	{
		if (_equippedFamiliar != null)
		{
			FamiliarBase.AddPoofAnimation(_level, _equippedFamiliar.Bbox.Center);
		}
	}

	private void SwitchFamiliar(bool isGoingRight)
	{
		Dictionary<int, InventoryFamiliar> inventory = _level.GameSave.Inventory.FamiliarInventory.Inventory;
		if (inventory.Count <= 1)
		{
			return;
		}
		EInventoryFamiliarType equippedFamiliarType = _equippedFamiliarType;
		EInventoryFamiliarType eInventoryFamiliarType = EInventoryFamiliarType.None;
		bool flag = false;
		int num = (int)equippedFamiliarType;
		for (int i = 0; i < 6; i++)
		{
			num += (isGoingRight ? 1 : (-1));
			if (num > 6)
			{
				num = 1;
			}
			else if (num < 1)
			{
				num = 6;
			}
			if (inventory.ContainsKey(num))
			{
				flag = true;
				eInventoryFamiliarType = (EInventoryFamiliarType)num;
				break;
			}
		}
		if (flag && eInventoryFamiliarType != equippedFamiliarType)
		{
			_level.GameSave.Inventory.EquippedFamiliar = eInventoryFamiliarType;
			ChangeFamiliar(eInventoryFamiliarType);
			AddFamiliarPoofAnimation();
		}
	}

	internal void ActivateFamiliar(PlayerIndex playerIndex)
	{
		if (_equippedFamiliar != null)
		{
			_equippedFamiliar.Activate(playerIndex);
		}
	}
}
