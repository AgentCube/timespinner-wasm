using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.Core;

public class SpriteSheet
{
	private readonly int _width;

	private readonly int _height;

	private readonly string _name;

	protected readonly Texture2D _texture;

	protected readonly List<Point> _frameStarts = new List<Point>();

	protected bool _isTextureAtlas;

	protected bool _isTileSet;

	protected int _frameCount = 1;

	private int _rowCount;

	protected int _directionGap;

	protected Point _frameSize;

	public bool IsTextureAtlas => _isTextureAtlas;

	public int Width => _width;

	public int Height => _height;

	public int FrameCount => _frameCount;

	public int RowWidth => _rowCount;

	public string Name => _name;

	public Texture2D Texture => _texture;

	public Point FrameSize => _frameSize;

	public List<Point> FrameStarts => _frameStarts;

	public int DirectionGap => _directionGap;

	protected SpriteSheet(string inName, Texture2D inTexture)
	{
		_name = inName;
		_texture = inTexture;
		_width = _texture.Width;
		_height = _texture.Height;
	}

	public SpriteSheet(string inName, Texture2D inTexture, Point inFrameSize)
		: this(inName, inTexture)
	{
		_frameSize = inFrameSize;
		if (inFrameSize == Point.Zero)
		{
			_frameSize = new Point(_texture.Width, _texture.Height);
		}
		LoadSheetFrameData();
	}

	private void LoadSheetFrameData()
	{
		_rowCount = (int)Math.Floor((float)_texture.Width / (float)_frameSize.X);
		int num = (int)Math.Floor((float)_texture.Height / (float)_frameSize.Y);
		if (_rowCount < 1)
		{
			_rowCount = 1;
			_frameSize.X = _texture.Width;
		}
		if (num < 1)
		{
			num = 1;
			_frameSize.Y = _texture.Height;
		}
		int y = 0;
		int x = 0;
		if (_isTileSet)
		{
			y = 1;
			x = 1;
		}
		Point item = new Point(x, y);
		_frameCount = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < _rowCount; j++)
			{
				_frameStarts.Add(item);
				item.X += _frameSize.X;
				_frameCount++;
			}
			item.X = x;
			item.Y += _frameSize.Y;
		}
		if (_frameCount % 2 == 0)
		{
			_directionGap = _frameCount / 2;
		}
		else
		{
			_directionGap = (int)Math.Floor((float)_frameCount / 2f);
		}
	}

	public virtual Rectangle GetFrameSource(int index)
	{
		Rectangle result = Rectangle.Empty;
		if (index >= 0)
		{
			Point point = FrameStarts[index];
			result = new Rectangle(point.X, point.Y, FrameSize.X, FrameSize.Y);
		}
		return result;
	}
}
