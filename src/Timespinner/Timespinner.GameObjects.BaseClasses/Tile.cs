using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.BaseClasses;

public class Tile : GameObject
{
	private readonly bool _isInvisibleSolidTile;

	private bool _isFlippedHorizontally;

	private bool _isFlippedVertically;

	protected ETileType _tileType;

	protected ESlopeType _slopeType;

	protected ETileSpecialType _specialType;

	protected int _tileIndex;

	protected Point _dictKey = Point.Zero;

	protected Color _drawColor = Color.White;

	protected Rectangle _drawSource;

	public bool IsFlippedVertically
	{
		get
		{
			return _isFlippedVertically;
		}
		set
		{
			_isFlippedVertically = value;
			RefreshSpriteEffects();
		}
	}

	public bool IsFlippedHorizontally
	{
		get
		{
			return _isFlippedHorizontally;
		}
		set
		{
			_isFlippedHorizontally = value;
			RefreshSpriteEffects();
		}
	}

	public ETileType Type => _tileType;

	public ESlopeType Slope => _slopeType;

	public ETileSpecialType Special => _specialType;

	public int TileIndex => _tileIndex;

	public Point DictKey => _dictKey;

	public SpriteEffects SpriteEffects { get; set; }

	public Tile(Point inPosition, Level inLevel, SpriteSheet inSprite, int inTileIndex, ETileType inType, ESlopeType inSlopeType, ETileSpecialType inSpikeType)
		: base(inPosition, inLevel)
	{
		_sprite = inSprite;
		_tileIndex = inTileIndex;
		_tileType = inType;
		_slopeType = inSlopeType;
		_specialType = inSpikeType;
		_dictKey = new Point(inPosition.X / 16, inPosition.Y / 16);
		_isInvisibleSolidTile = _tileIndex >= 512;
		if (!_isInvisibleSolidTile)
		{
			_drawSource = _sprite.GetFrameSource(_tileIndex);
		}
		_bbox = new Rectangle(_position.X, _position.Y, 16, 16);
	}

	public Tile(Point inPosition, Level inLevel, SpriteSheet inSprite, int inTileIndex, ETileType inType)
		: this(inPosition, inLevel, inSprite, inTileIndex, inType, ESlopeType.None)
	{
	}

	public Tile(Point inPosition, Level inLevel, SpriteSheet inSprite, int inTileIndex, ETileType inType, ETileSpecialType inSpikeType)
		: this(inPosition, inLevel, inSprite, inTileIndex, inType, ESlopeType.None, inSpikeType)
	{
	}

	public Tile(Point inPosition, Level inLevel, SpriteSheet inSprite, int inTileIndex, ETileType inType, ESlopeType inSlopeType)
		: this(inPosition, inLevel, inSprite, inTileIndex, inType, inSlopeType, ETileSpecialType.None)
	{
	}

	internal void SetDrawColor(Color newColor)
	{
		_drawColor = newColor;
	}

	private void RefreshSpriteEffects()
	{
		SpriteEffects = (IsFlippedHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (IsFlippedVertically ? SpriteEffects.FlipVertically : SpriteEffects.None);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isInvisibleSolidTile && Special != ETileSpecialType.HorizontalSpike && Special != ETileSpecialType.CamBlock)
		{
			Vector2 value = Vector2.Subtract(_level.CameraPosition, new Vector2(_position.X, _position.Y));
			value = Vector2.Multiply(value, _level.CameraZoom);
			spriteBatch.Draw(_sprite.Texture, Vector2.Subtract(_level.LevelRenderCenter, value), _drawSource, _drawColor, 0f, Vector2.Zero, _level.CameraZoom, SpriteEffects, 0f);
		}
	}

	public int LookupTileHeight(float posX)
	{
		float num = (posX - (float)_position.X) / 16f;
		if (num < 0f || num > 1f)
		{
			return -1;
		}
		switch (Slope)
		{
		case ESlopeType.Left45:
			return (int)MathHelper.Lerp(16f, 0f, num);
		case ESlopeType.Right45:
			return (int)MathHelper.Lerp(0f, 16f, num);
		case ESlopeType.Left30High:
			return (int)MathHelper.Lerp(8f, 0f, num);
		case ESlopeType.Left30Low:
			return (int)MathHelper.Lerp(16f, 8f, num);
		case ESlopeType.Right30High:
			return (int)MathHelper.Lerp(0f, 8f, num);
		case ESlopeType.Right30Low:
			return (int)MathHelper.Lerp(8f, 16f, num);
		case ESlopeType.Right14HighHigh:
			return (int)MathHelper.Lerp(0f, 4f, num);
		case ESlopeType.Right14HighLow:
			return (int)MathHelper.Lerp(4f, 8f, num);
		case ESlopeType.Right14LowHigh:
			return (int)MathHelper.Lerp(8f, 12f, num);
		case ESlopeType.Right14LowLow:
			return (int)MathHelper.Lerp(12f, 16f, num);
		case ESlopeType.Left14HighHigh:
			return (int)MathHelper.Lerp(4f, 0f, num);
		case ESlopeType.Left14HighLow:
			return (int)MathHelper.Lerp(8f, 4f, num);
		case ESlopeType.Left14LowHigh:
			return (int)MathHelper.Lerp(12f, 8f, num);
		case ESlopeType.Left14LowLow:
			return (int)MathHelper.Lerp(16f, 12f, num);
		default:
			if (Type != ETileType.Slope)
			{
				return 0;
			}
			return -1;
		}
	}

	public float GetSlopeAngle()
	{
		return Slope switch
		{
			ESlopeType.Left45 => -1.5f, 
			ESlopeType.Right45 => 1.5f, 
			ESlopeType.Left30High => -1f, 
			ESlopeType.Left30Low => -1f, 
			ESlopeType.Right30High => 1f, 
			ESlopeType.Right30Low => 1f, 
			ESlopeType.Right14HighHigh => 0.5f, 
			ESlopeType.Right14HighLow => 0.5f, 
			ESlopeType.Right14LowHigh => 0.5f, 
			ESlopeType.Right14LowLow => 0.5f, 
			ESlopeType.Left14HighHigh => -0.5f, 
			ESlopeType.Left14HighLow => -0.5f, 
			ESlopeType.Left14LowHigh => -0.5f, 
			ESlopeType.Left14LowLow => -0.5f, 
			_ => 0f, 
		};
	}

	public float GetSlopeRadians()
	{
		float result = 0f;
		float slopeAngle = GetSlopeAngle();
		if (slopeAngle == 0.5f || slopeAngle == -0.5f)
		{
			result = MathHelper.ToRadians(14f) * (slopeAngle / 0.5f);
		}
		else if (slopeAngle == 1f || slopeAngle == -1f)
		{
			result = MathHelper.ToRadians(28f) * (slopeAngle / 1f);
		}
		else if (slopeAngle == 1.5f || slopeAngle == -1.5f)
		{
			result = MathHelper.ToRadians(45f) * (slopeAngle / 1.5f);
		}
		return result;
	}

	public static ESlopeType LookupSlopeType(int tileID, bool isFlippedX)
	{
		ESlopeType result = ESlopeType.None;
		switch (tileID)
		{
		case 0:
			result = ((!isFlippedX) ? ESlopeType.Left45 : ESlopeType.Right45);
			break;
		case 1:
			result = (isFlippedX ? ESlopeType.Left45 : ESlopeType.Right45);
			break;
		case 2:
			result = ((!isFlippedX) ? ESlopeType.Left30Low : ESlopeType.Right30Low);
			break;
		case 3:
			result = ((!isFlippedX) ? ESlopeType.Left30High : ESlopeType.Right30High);
			break;
		case 4:
			result = ((!isFlippedX) ? ESlopeType.Right30High : ESlopeType.Left30High);
			break;
		case 5:
			result = ((!isFlippedX) ? ESlopeType.Right30Low : ESlopeType.Left30Low);
			break;
		case 6:
			result = ((!isFlippedX) ? ESlopeType.Left14LowLow : ESlopeType.Right14LowLow);
			break;
		case 7:
			result = ((!isFlippedX) ? ESlopeType.Left14LowHigh : ESlopeType.Right14LowHigh);
			break;
		case 8:
			result = ((!isFlippedX) ? ESlopeType.Left14HighLow : ESlopeType.Right14HighLow);
			break;
		case 9:
			result = ((!isFlippedX) ? ESlopeType.Left14HighHigh : ESlopeType.Right14HighHigh);
			break;
		case 10:
			result = ((!isFlippedX) ? ESlopeType.Right14HighHigh : ESlopeType.Left14HighHigh);
			break;
		case 11:
			result = ((!isFlippedX) ? ESlopeType.Right14HighLow : ESlopeType.Left14HighLow);
			break;
		case 12:
			result = ((!isFlippedX) ? ESlopeType.Right14LowHigh : ESlopeType.Left14LowHigh);
			break;
		case 13:
			result = ((!isFlippedX) ? ESlopeType.Right14LowLow : ESlopeType.Left14LowLow);
			break;
		}
		return result;
	}

	public static Tile FromSpecification(TileSpecification tile, Level level)
	{
		Point inPosition = new Point(tile.X * 16, tile.Y * 16);
		Tile tile2 = null;
		switch (tile.Layer)
		{
		case ETileLayerType.Bottom:
			tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Passable);
			break;
		case ETileLayerType.Middle:
		{
			if (tile.ID < 512)
			{
				if (tile.ID < 96 || tile.ID > 128)
				{
					tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Solid);
					break;
				}
				if (tile.ID < 98)
				{
					tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Solid, ETileSpecialType.CamBlock);
					break;
				}
				if (tile.ID < 112)
				{
					tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Platform);
					break;
				}
				if (tile.ID < 114)
				{
					tile2 = ((tile.ID == 113) ? new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Solid, ETileSpecialType.HorizontalSpike) : new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Solid, ETileSpecialType.VerticalSpike));
					break;
				}
				ESlopeType inSlopeType = LookupSlopeType(tile.ID - 114, tile.IsFlippedHorizontally);
				tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Slope, inSlopeType);
				break;
			}
			ESolidTileType eSolidTileType = (ESolidTileType)(tile.ID - 512);
			switch (eSolidTileType)
			{
			case ESolidTileType.Solid:
				tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Solid);
				break;
			case ESolidTileType.Platform:
				tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Platform);
				break;
			case ESolidTileType.HorizontalSpike:
				tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Solid, ETileSpecialType.HorizontalSpike);
				break;
			case ESolidTileType.VerticalSpike:
				tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Solid, ETileSpecialType.VerticalSpike);
				break;
			case ESolidTileType.Slope45:
			case ESolidTileType.Slope30_1:
			case ESolidTileType.Slope30_2:
			case ESolidTileType.Slope15_1:
			case ESolidTileType.Slope15_2:
			case ESolidTileType.Slope15_3:
			case ESolidTileType.Slope15_4:
			{
				int num = tile.ID - 512 - 4;
				if (eSolidTileType == ESolidTileType.Slope45)
				{
					num = 0;
				}
				else if (eSolidTileType == ESolidTileType.Slope30_1 || eSolidTileType == ESolidTileType.Slope30_2)
				{
					num++;
				}
				else if (eSolidTileType >= ESolidTileType.Slope15_1 || eSolidTileType <= ESolidTileType.Slope15_4)
				{
					num += 3;
				}
				ESlopeType inSlopeType2 = LookupSlopeType(num, tile.IsFlippedHorizontally);
				tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Slope, inSlopeType2);
				break;
			}
			}
			break;
		}
		case ETileLayerType.Top:
			tile2 = new Tile(inPosition, level, level.CurrentTileset, tile.ID, ETileType.Passable);
			break;
		}
		if (tile2 != null)
		{
			tile2.IsFlippedVertically = tile.IsFlippedVertically;
			tile2.IsFlippedHorizontally = tile.IsFlippedHorizontally;
		}
		return tile2;
	}

	internal static bool IsSlopeFacingLeft(ESlopeType slope)
	{
		if (slope != ESlopeType.Left14HighHigh && slope != ESlopeType.Left14HighLow && slope != ESlopeType.Left14LowHigh && slope != ESlopeType.Left14LowLow && slope != ESlopeType.Left30High && slope != ESlopeType.Left30Low)
		{
			return slope == ESlopeType.Left45;
		}
		return true;
	}
}
