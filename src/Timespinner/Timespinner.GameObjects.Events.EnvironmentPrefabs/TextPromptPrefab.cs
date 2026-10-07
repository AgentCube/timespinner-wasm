using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal abstract class TextPromptPrefab : EnvironmentPrefabBase
{
	private const int TextPromptOffsetY = -96;

	private static readonly Color ShadowColor = new Color(16, 16, 16);

	private readonly int _textPromptOffsetX;

	private readonly string _textPromptText;

	private readonly Color _baseTextColor;

	private readonly SpriteFont _font;

	private bool _hasBeenUsed;

	private bool _isBeingTriggerdByPlayer;

	private float _textAlpha;

	private Point _textPosition;

	protected TextPromptPrefab(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType, string text, Color textColor)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_textPromptText = text;
		_baseTextColor = textColor;
		_font = _level.GCM.ActiveFont;
		_textPromptOffsetX = -(int)(_font.MeasureString(_textPromptText).X * 0.5f);
		Bbox = new Rectangle(0, 0, 48, 48);
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		_isSolid = false;
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		_doAppendagesMatchImageFacing = false;
	}

	internal virtual void OnActivate()
	{
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		if (_isBeingTriggerdByPlayer)
		{
			if (_textAlpha < 1f)
			{
				_textAlpha += delta;
				if (_textAlpha > 1f)
				{
					_textAlpha = 1f;
				}
			}
		}
		else if (_textAlpha > 0f)
		{
			_textAlpha -= delta;
			if (_textAlpha < 0f)
			{
				_textAlpha = 0f;
			}
		}
		_isBeingTriggerdByPlayer = false;
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!_hasBeenUsed && who is Protagonist protagonist)
		{
			_isBeingTriggerdByPlayer = true;
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Center.Y));
			if (protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				_hasBeenUsed = true;
				OnActivate();
			}
		}
		return base.TriggerEvent(who, depth);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_textAlpha > 0f)
		{
			_textPosition = new Point(Position.X + _textPromptOffsetX, Position.Y + -96);
			Vector2 value = Vector2.Subtract(base.Level.CameraPosition, new Vector2(_textPosition.X, _textPosition.Y));
			value = Vector2.Subtract(base.Level.LevelRenderCenter, Vector2.Multiply(value, base.Level.CameraZoom));
			DrawingEx.DrawString(spriteBatch, _font, _textPromptText, Vector2.Add(value, Vector2.One), ShadowColor * _textAlpha);
			DrawingEx.DrawString(spriteBatch, _font, _textPromptText, value, _baseTextColor * _textAlpha);
		}
		base.Draw(spriteBatch);
	}
}
