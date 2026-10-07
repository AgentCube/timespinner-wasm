using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class CharacterQuestMenuEntryCollection : MenuEntryCollection
{
	private const int QuestsCappedFrameIndex = 146;

	private const int QuestFrameDrawPositionX = -6;

	private const int QuestFrameDrawPositionY = -7;

	private const int QuestFrameOffsetY = 15;

	private const int QuestStateFrameOffsetX = 89;

	private const int QuestStateFrameOffsetY = 3;

	private const int QuestStateIndexOffsetX = 10;

	private const int PortraitDrawPositionX = 120;

	private const int PortraitDrawPositionY = 63;

	private const int QuestHeaderDrawPositionX = -8;

	private const int QuestTitleDrawPositionX = 0;

	private const int SelectedQuestTitleMaxWidth = 128;

	private const int SelectedQuestStatusMaxWidth = 108;

	private const int LatinQuestHeaderDrawPositionY = 80;

	private const int LatinQuestTitleDrawPositionY = 90;

	private const int LatinQuestStatusHeaderDrawPositionY = 100;

	private const int LatinQuestStatusDrawPositionY = 110;

	private const int AsianQuestHeaderDrawPositionY = 72;

	private const int AsianQuestTitleDrawPositionY = 86;

	private const int AsianQuestStatusHeaderDrawPositionY = 100;

	private const int AsianQuestStatusDrawPositionY = 114;

	private const string NPC_Name_Key_Format = "inv_jou_quest_{0}";

	private const string NPC_Description_Key_Format = "inv_jou_quest_{0}_desc";

	private const string NPC_Quest_Title_Key_Format = "inv_jou_quest_{0}_{1}";

	private const string NPC_Quest_Description_Key_Format = "inv_jou_quest_{0}_{1}_desc";

	private const string Quest_Title_Key_Format = "inv_jou_quest_Title";

	private const string Quest_Status_Key_Format = "inv_jou_quest_Status";

	private const string Quest_Status_State_Key_Format = "inv_jou_quest_Status_{0}";

	private const string NPC_SickSoldier_SickName_Key = "inv_jou_quest_SickSoldier_sick";

	private const string NPC_SickSoldier_SickDescription_Key = "inv_jou_quest_SickSoldier_sick_desc";

	private static readonly Color TextShadowColor = new Color(48, 48, 16);

	private readonly string _questStatusText;

	private readonly string _questTitleText;

	private readonly SpriteSheet _frameSprite;

	private readonly SpriteSheet _portraitsSprite;

	private readonly List<NPCBase.ENPCType> _npcList = new List<NPCBase.ENPCType>();

	private readonly List<Rectangle> _portraitFrameSources = new List<Rectangle>();

	private readonly List<QuestMenuEntryCollection> _questMenuEntryCollections = new List<QuestMenuEntryCollection>();

	private readonly Dictionary<int, List<NPCBase.EQuestStateType>> _questStates;

	private readonly Action<MenuEntryCollection> _clickCharacterAction;

	private int _questFrameDrawX;

	private int _questFrameDrawY;

	private int _questFrameOffsetY;

	private int _questStateFrameOffsetX;

	private int _questStateFrameOffsetY;

	private int _questStateIndexOffsetX;

	private int _portraitDrawPositionX;

	private int _portraitDrawPositionY;

	private int _selectedIndex = -1;

	private int _zoom;

	private Vector2 _questHeaderDrawPosition;

	private Vector2 _questTitleDrawPosition;

	private Vector2 _questStatusHeaderDrawPosition;

	private Vector2 _questStatusDrawPosition;

	private ScrollableTextBlock _selectedQuestTitle;

	private ScrollableTextBlock _selectedQuestStatus;

	private QuestMenuEntryCollection _selectedQuestCollection;

	internal List<QuestMenuEntryCollection> QuestMenuEntryCollection => _questMenuEntryCollections;

	public CharacterQuestMenuEntryCollection(Dictionary<int, List<NPCBase.EQuestStateType>> questStates, SpriteSheet frameSprite, SpriteSheet portraitsSprite, GameSave saveFile, Action<MenuEntryCollection> clickCharacterAction)
	{
		_questStates = questStates;
		_frameSprite = frameSprite;
		_clickCharacterAction = clickCharacterAction;
		_portraitsSprite = portraitsSprite;
		base.ScrollRowHeight = 20;
		_questTitleText = Loc.Get("inv_jou_quest_Title");
		_questStatusText = Loc.Get("inv_jou_quest_Status");
		foreach (KeyValuePair<int, List<NPCBase.EQuestStateType>> questState in _questStates)
		{
			NPCBase.ENPCType key = (NPCBase.ENPCType)questState.Key;
			_npcList.Add(key);
			string text = Loc.Get($"inv_jou_quest_{key}");
			string description = Loc.Get($"inv_jou_quest_{key}_desc");
			if (key == NPCBase.ENPCType.SickSoldier)
			{
				List<NPCBase.EQuestStateType> value = questState.Value;
				if (value.Count == 0 || value[0] != NPCBase.EQuestStateType.Closed)
				{
					text = Loc.Get("inv_jou_quest_SickSoldier_sick");
					description = Loc.Get("inv_jou_quest_SickSoldier_sick_desc");
				}
			}
			MenuEntry menuEntry = new MenuEntry(text)
			{
				Description = description
			};
			menuEntry.Selected += delegate
			{
				OnSelectCharacter();
			};
			QuestMenuEntryCollection questMenuEntryCollection = new QuestMenuEntryCollection(OnSelectedQuestChanged)
			{
				ColumnCount = 5,
				ScrollRowHeight = 20
			};
			int num = 1;
			foreach (NPCBase.EQuestStateType item in questState.Value)
			{
				if (item == NPCBase.EQuestStateType.Unknown || item == NPCBase.EQuestStateType.QuestsCapped)
				{
					continue;
				}
				QuestMenuEntry questMenuEntry = new QuestMenuEntry(Loc.Get($"inv_jou_quest_{key}_{num}"), item)
				{
					Description = Loc.Get($"inv_jou_quest_{key}_{num}_desc")
				};
				if (item == NPCBase.EQuestStateType.InProgress || item == NPCBase.EQuestStateType.Started)
				{
					int questID = num - 1;
					Point ratioQuestRatio = NPCBase.GetRatioQuestRatio(key, questID, saveFile);
					if (ratioQuestRatio.X > -1)
					{
						questMenuEntry.Description += $" ({ratioQuestRatio.X}/{ratioQuestRatio.Y})";
					}
				}
				questMenuEntryCollection.AddQuestEntry(questMenuEntry);
				num++;
			}
			if (num > 1)
			{
				questMenuEntryCollection.SelectedIndex = questMenuEntryCollection.Entries.Count - 1;
			}
			_questMenuEntryCollections.Add(questMenuEntryCollection);
			base.Entries.Add(menuEntry);
			if (_portraitsSprite != null)
			{
				DialogueBox.ECharacterPortraitType portraitType = DialogueBox.PortraitFromNPCType(key);
				_portraitFrameSources.Add(DialogueBox.FrameSourceFromPortrait(portraitType, _portraitsSprite));
			}
		}
	}

	private void OnSelectedQuestChanged(QuestMenuEntry questMenuEntry)
	{
		_selectedQuestTitle = new ScrollableTextBlock(base.Font, 128, _questTitleDrawPosition, isTextCentered: false);
		_selectedQuestTitle.SetText(questMenuEntry.QuestTitle);
		_selectedQuestStatus = new ScrollableTextBlock(base.Font, 108, _questStatusDrawPosition, isTextCentered: false);
		_selectedQuestStatus.SetText(Loc.Get($"inv_jou_quest_Status_{questMenuEntry.QuestState}"));
	}

	private void RefreshQuestDisplays()
	{
		if (_selectedQuestCollection != null && _selectedQuestCollection.SelectedIndex < _selectedQuestCollection.Entries.Count)
		{
			OnSelectedQuestChanged(_selectedQuestCollection.QuestEntries[_selectedQuestCollection.SelectedIndex]);
		}
	}

	private void OnSelectCharacter()
	{
		if (_clickCharacterAction != null && base.SelectedIndex < _questMenuEntryCollections.Count)
		{
			QuestMenuEntryCollection obj = (_selectedQuestCollection = _questMenuEntryCollections[base.SelectedIndex]);
			_clickCharacterAction(obj);
		}
	}

	public override void Update(float delta, bool isScreenActive, float transitionPercentage)
	{
		base.Update(delta, isScreenActive, transitionPercentage);
		foreach (QuestMenuEntryCollection questMenuEntryCollection in _questMenuEntryCollections)
		{
			questMenuEntryCollection.Update(delta, isScreenActive, transitionPercentage);
		}
		if (_selectedIndex != base.SelectedIndex && base.SelectedIndex < _questMenuEntryCollections.Count)
		{
			_selectedQuestCollection = _questMenuEntryCollections[base.SelectedIndex];
			RefreshQuestDisplays();
			_selectedIndex = base.SelectedIndex;
		}
	}

	internal void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		bool isAsianLocale = Loc.IsAsianLocale;
		int num = (isAsianLocale ? 72 : 80);
		int num2 = (isAsianLocale ? 86 : 90);
		int num3 = (isAsianLocale ? 100 : 100);
		int num4 = (isAsianLocale ? 114 : 110);
		_questFrameDrawX = (int)(base.DrawPosition.X + (float)(-6 * _zoom));
		_questFrameDrawY = (int)(base.DrawPosition.Y + (float)(-7 * _zoom));
		_questFrameOffsetY = 15 * _zoom;
		_questStateFrameOffsetX = 89 * _zoom;
		_questStateFrameOffsetY = 3 * _zoom;
		_questStateIndexOffsetX = 10 * _zoom;
		_portraitDrawPositionX = (int)(base.DrawPosition.X + (float)(120 * _zoom));
		_portraitDrawPositionY = (int)(base.DrawPosition.Y + (float)(63 * _zoom));
		_questHeaderDrawPosition = new Vector2(base.DrawPosition.X + (float)(-8 * _zoom), base.DrawPosition.Y + (float)(num * _zoom));
		_questTitleDrawPosition = new Vector2(base.DrawPosition.X + 0f, base.DrawPosition.Y + (float)(num2 * _zoom));
		_questStatusHeaderDrawPosition = new Vector2(_questHeaderDrawPosition.X, base.DrawPosition.Y + (float)(num3 * _zoom));
		_questStatusDrawPosition = new Vector2(_questTitleDrawPosition.X, base.DrawPosition.Y + (float)(num4 * _zoom));
		int num5 = 0;
		foreach (QuestMenuEntryCollection questMenuEntryCollection in _questMenuEntryCollections)
		{
			questMenuEntryCollection.DrawPosition = new Vector2(_questFrameDrawX + _questStateFrameOffsetX, _questFrameDrawY + num5 * _questFrameOffsetY + 7 * _zoom);
			questMenuEntryCollection.SetColumnWidth(_questStateIndexOffsetX, _zoom);
			num5++;
		}
		RefreshQuestDisplays();
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		if (base.IsVisible)
		{
			int num = 0;
			Rectangle frameSource = _frameSprite.GetFrameSource(113);
			Color white = Color.White;
			for (int i = 0; i < 5; i++)
			{
				if (!_questStates.ContainsKey(i))
				{
					continue;
				}
				List<NPCBase.EQuestStateType> list = _questStates[i];
				int num2 = _questFrameDrawY + num;
				spriteBatch.Draw(_frameSprite.Texture, new Vector2(_questFrameDrawX, num2), frameSource, white, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
				int num3 = _questFrameDrawX + _questStateFrameOffsetX;
				int num4 = num2 + _questStateFrameOffsetY;
				int num5 = 0;
				foreach (NPCBase.EQuestStateType item in list)
				{
					if (item > NPCBase.EQuestStateType.Unknown && num5 < 5)
					{
						Rectangle frameSource2 = _frameSprite.GetFrameSource((int)(113 + item));
						if (item == NPCBase.EQuestStateType.QuestsCapped)
						{
							frameSource2 = _frameSprite.GetFrameSource(146);
						}
						spriteBatch.Draw(_frameSprite.Texture, new Vector2(num3, num4), frameSource2, white, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
					}
					num3 += _questStateIndexOffsetX;
					num5++;
				}
				num += _questFrameOffsetY;
			}
			if (_selectedQuestTitle != null)
			{
				DrawShadowedText(spriteBatch, base.Font, _questTitleText, _questHeaderDrawPosition, MenuEntry.UnselectedColor, zoom);
				_selectedQuestTitle.Draw(spriteBatch, MenuEntry.SelectedColor, TextShadowColor, null);
				DrawShadowedText(spriteBatch, base.Font, _questStatusText, _questStatusHeaderDrawPosition, MenuEntry.UnselectedColor, zoom);
				_selectedQuestStatus.Draw(spriteBatch, MenuEntry.SelectedColor, TextShadowColor, null);
			}
		}
		base.Draw(spriteBatch, zoom);
	}

	private static void DrawShadowedText(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float zoom)
	{
		DrawingEx.DrawString(spriteBatch, font, text, new Vector2(position.X, position.Y + zoom), TextShadowColor, Vector2.Zero, zoom);
		DrawingEx.DrawString(spriteBatch, font, text, position, color, Vector2.Zero, zoom);
	}

	internal void DrawPortrait(SpriteBatch spriteBatch, Color drawColor)
	{
		if (base.SelectedIndex < _portraitFrameSources.Count)
		{
			spriteBatch.Draw(_portraitsSprite.Texture, new Vector2(_portraitDrawPositionX, _portraitDrawPositionY), _portraitFrameSources[base.SelectedIndex], drawColor, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		}
	}
}
