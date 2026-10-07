using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.Core;

public sealed class TextureAtlas : SpriteSheet
{
	private readonly List<Rectangle> _frameSources = new List<Rectangle>();

	private readonly List<TextureAtlasFrame> _atlasFrames;

	public List<TextureAtlasFrame> AtlasFrames => _atlasFrames;

	public TextureAtlas(string inName, Texture2D inTexture, List<TextureAtlasFrame> atlasFrames)
		: base(inName, inTexture)
	{
		_isTextureAtlas = true;
		_atlasFrames = atlasFrames;
		LoadAtlasFrameData();
	}

	private void LoadAtlasFrameData()
	{
		_frameCount = 0;
		foreach (TextureAtlasFrame atlasFrame in _atlasFrames)
		{
			int num = (int)Math.Floor((float)_texture.Width / (float)atlasFrame.FrameSize.X);
			if (base.FrameSize == Point.Zero)
			{
				_frameSize = atlasFrame.FrameSize;
			}
			Point point;
			if (atlasFrame.StartIndex == 0)
			{
				point = atlasFrame.StartCoordinates;
			}
			else
			{
				int num2 = atlasFrame.StartIndex % num;
				int num3 = atlasFrame.StartIndex / num;
				point = new Point(num2 * atlasFrame.FrameSize.X, num3 * atlasFrame.FrameSize.Y);
			}
			Point item = point;
			int num4 = atlasFrame.StartIndex % num;
			for (int i = 0; i < atlasFrame.Count; i++)
			{
				_frameStarts.Add(item);
				_frameSources.Add(new Rectangle(item.X, item.Y, atlasFrame.FrameSize.X, atlasFrame.FrameSize.Y));
				_frameCount++;
				num4++;
				if (num4 < atlasFrame.RowWidth)
				{
					item.X += atlasFrame.FrameSize.X;
					continue;
				}
				item.X = (atlasFrame.DoesNewRowUseStartX ? point.X : 0);
				item.Y += atlasFrame.FrameSize.Y;
				num4 = 0;
			}
		}
	}

	public override Rectangle GetFrameSource(int index)
	{
		if (index < 0 || index >= _frameCount)
		{
			return Rectangle.Empty;
		}
		return _frameSources[index];
	}
}
