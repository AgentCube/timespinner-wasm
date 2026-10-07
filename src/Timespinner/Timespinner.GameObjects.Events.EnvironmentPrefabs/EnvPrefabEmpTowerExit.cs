using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabEmpTowerExit : EnvironmentPrefabBase
{
	private const int TextPromptOffsetY = -96;

	private const int TextDrawLeftMinimum = 17;

	private const string TextKey = "Tutorial_Leave";

	private static readonly Color ShadowColor = new Color(16, 16, 16);

	private static readonly Color TextDrawColor = new Color(220, 160, 200);

	private readonly int _textPromptOffsetX;

	private readonly string _textPromptText;

	private readonly SpriteFont _font;

	private bool _isBeingTriggerdByPlayer;

	private float _textAlpha;

	private Point _textPosition;

	public EnvPrefabEmpTowerExit(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_textPromptText = Loc.Get("Tutorial_Leave");
		_font = _level.GCM.ActiveFont;
		_textPromptOffsetX = -(int)(_font.MeasureString(_textPromptText).X * 0.5f);
		base.DrawPlane = EDrawPlane.Front;
		Bbox = new Rectangle(0, 0, 48, 80);
		SnapBboxToPosition();
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
		if (who is Protagonist)
		{
			_isBeingTriggerdByPlayer = true;
		}
		return base.TriggerEvent(who, depth);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_textAlpha > 0f)
		{
			int num = Position.X + _textPromptOffsetX;
			if (num < 17)
			{
				num = 17;
			}
			_textPosition = new Point(num, Position.Y + -96);
			Vector2 value = Vector2.Subtract(base.Level.CameraPosition, new Vector2(_textPosition.X, _textPosition.Y));
			value = Vector2.Subtract(base.Level.LevelRenderCenter, Vector2.Multiply(value, base.Level.CameraZoom));
			DrawingEx.DrawString(spriteBatch, _font, _textPromptText, Vector2.Add(value, Vector2.One), ShadowColor * _textAlpha);
			DrawingEx.DrawString(spriteBatch, _font, _textPromptText, value, TextDrawColor * _textAlpha);
		}
		base.Draw(spriteBatch);
	}
}
