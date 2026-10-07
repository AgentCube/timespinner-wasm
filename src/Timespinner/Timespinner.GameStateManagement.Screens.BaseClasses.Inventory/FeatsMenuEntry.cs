using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Inventory;

internal class FeatsMenuEntry : MenuEntry
{
	private const int BackingWidth = 174;

	private const int BackingHeight = 30;

	private const int BackingOffsetX = -29;

	private const int BackingOffsetY = -15;

	private const int IconHolderOffsetX = -25;

	private const int IconHolderOffsetY = -11;

	private const int IconOffsetX = 3;

	private const int IconOffsetY = 3;

	private const int Anim_BossFeatNoHitIndex = 240;

	private const int Anim_BossFeatNoTimeIndex = 241;

	private const int Anim_BossFeatNoHitPerfectIndex = 242;

	private const int Anim_BossFeatNoTimePerfectIndex = 243;

	private const int Anim_BossFeatPerfectPlusIndex = 244;

	private const int BossFeatIconOffsetX = 123;

	private const int BossFeatIconBufferX = 23;

	private const int BossFeatPlusOffsetX = 134;

	private const int BossFeatMarginX = -20;

	private static readonly Color GrayedOutColor = new Color(160, 160, 160);

	private readonly bool _isUnlocked;

	private readonly bool _isBossFeat;

	private readonly bool _isBossNoHit;

	private readonly bool _isBossNoTime;

	private readonly bool _isBossPerfect;

	private readonly int _iconIndex;

	private readonly Rectangle _iconFrameSource;

	private readonly Rectangle _bossNoHitFrameSource;

	private readonly Rectangle _bossNoTimeFrameSource;

	private readonly Rectangle _bossPerfectPlusFrameSource;

	private readonly GameFeat _feat;

	private readonly SpriteSheet _uiSprite;

	private readonly SpriteSheet _iconsSprite;

	public FeatsMenuEntry(string text, GameFeat feat, SpriteSheet uiSprite, SpriteSheet iconsSprite)
		: base(text)
	{
		_feat = feat;
		_uiSprite = uiSprite;
		_iconsSprite = iconsSprite;
		_iconIndex = _feat.GetIconIndex();
		_iconFrameSource = _iconsSprite.GetFrameSource(_iconIndex);
		_isUnlocked = _feat.IsUnlocked;
		base.BaseDrawColor = (_isUnlocked ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
		_isBossFeat = feat.IsBoss;
		if (_isBossFeat)
		{
			_isBossNoHit = (feat.ProgressValue & 2) != 0;
			_isBossNoTime = (feat.ProgressValue & 4) != 0;
			_isBossPerfect = (feat.ProgressValue & 8) != 0;
			if (_isBossPerfect)
			{
				_bossNoHitFrameSource = _iconsSprite.GetFrameSource(242);
				_bossNoTimeFrameSource = _iconsSprite.GetFrameSource(243);
			}
			else
			{
				_bossNoHitFrameSource = _iconsSprite.GetFrameSource(240);
				_bossNoTimeFrameSource = _iconsSprite.GetFrameSource(241);
			}
			_bossPerfectPlusFrameSource = _iconsSprite.GetFrameSource(244);
			int num = 0;
			if (_isBossNoHit)
			{
				num += -20;
			}
			if (_isBossNoTime)
			{
				num += -20;
			}
			base.EntryTextMarginX = num;
		}
	}

	public void Draw(SpriteBatch spriteBatch, SpriteFont font, float zoom, float drawAlpha)
	{
		int num = (int)zoom;
		int num2 = -29 * num;
		int num3 = -15 * num;
		int width = 174 * num;
		int height = 30 * num;
		int num4 = -25 * num;
		int num5 = -11 * num;
		Color color = (_isUnlocked ? Color.White : GrayedOutColor);
		Color color2 = color * drawAlpha;
		Vector2 drawPosition = base.DrawPosition;
		Point point = new Point((int)(drawPosition.X + (float)num2), (int)(drawPosition.Y + (float)num3));
		SpriteEffects[] array = new SpriteEffects[9];
		int[] frames = new int[9] { 29, -1, 29, -1, 0, -1, 29, -1, 29 };
		if (_isUnlocked)
		{
			frames = new int[9] { 28, 23, 28, -1, 0, -1, 28, 23, 28 };
		}
		array[1] = SpriteEffects.FlipVertically;
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, new Rectangle(point.X, point.Y, width, height), color2, _uiSprite, zoom, frames, array, shouldTile: true);
		Vector2 position = new Vector2(base.DrawPosition.X + (float)num4, base.DrawPosition.Y + (float)num5);
		Rectangle frameSource = _uiSprite.GetFrameSource(149);
		spriteBatch.Draw(_uiSprite.Texture, position, frameSource, color2, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		if (_isUnlocked)
		{
			int num6 = 3 * num;
			int num7 = 3 * num;
			Vector2 position2 = new Vector2(position.X + (float)num6, position.Y + (float)num7);
			spriteBatch.Draw(_iconsSprite.Texture, position2, _iconFrameSource, color2, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			if (_isBossFeat)
			{
				if (!_isBossPerfect)
				{
					if (_isBossNoHit)
					{
						int num8 = ((!_isBossNoTime) ? 23 : 0);
						spriteBatch.Draw(_iconsSprite.Texture, new Vector2(position2.X + (float)(123 + num8) * zoom, position2.Y), _bossNoHitFrameSource, color2, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
					}
					if (_isBossNoTime)
					{
						spriteBatch.Draw(_iconsSprite.Texture, new Vector2(position2.X + 146f * zoom, position2.Y), _bossNoTimeFrameSource, color2, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
					}
				}
				else
				{
					spriteBatch.Draw(_iconsSprite.Texture, new Vector2(position2.X + 123f * zoom, position2.Y), _bossNoHitFrameSource, color2, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
					spriteBatch.Draw(_iconsSprite.Texture, new Vector2(position2.X + 146f * zoom, position2.Y), _bossNoTimeFrameSource, color2, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
					spriteBatch.Draw(_iconsSprite.Texture, new Vector2(position2.X + 134f * zoom, position2.Y), _bossPerfectPlusFrameSource, color2, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
				}
			}
		}
		base.Draw(spriteBatch, font, zoom);
	}
}
