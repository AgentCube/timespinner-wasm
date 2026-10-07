using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.BaseClasses;

internal class TileSwath
{
	private const int TileSize = 16;

	private readonly Rectangle _area;

	private readonly Rectangle _drawSource;

	private readonly SpriteSheet _sprite;

	private readonly Level _level;

	internal Rectangle Area => _area;

	internal TileSwath(TileSwathSpecification specification, Level level, SpriteSheet sprite)
	{
		_area = new Rectangle(specification.X, specification.Y, specification.Width, specification.Height);
		_level = level;
		_sprite = sprite;
		_drawSource = _sprite.GetFrameSource(specification.ID);
	}

	internal void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
	{
		Rectangle rectangle = Rectangle.Intersect(visibleArea, _area);
		for (int i = rectangle.Left; i < rectangle.Right; i++)
		{
			for (int j = rectangle.Top; j < rectangle.Bottom; j++)
			{
				Vector2 value = Vector2.Subtract(_level.CameraPosition, new Vector2(i * 16, j * 16));
				value = Vector2.Multiply(value, _level.CameraZoom);
				spriteBatch.Draw(_sprite.Texture, Vector2.Subtract(_level.LevelRenderCenter, value), _drawSource, Color.White, 0f, Vector2.Zero, _level.CameraZoom, SpriteEffects.None, 0f);
			}
		}
	}
}
