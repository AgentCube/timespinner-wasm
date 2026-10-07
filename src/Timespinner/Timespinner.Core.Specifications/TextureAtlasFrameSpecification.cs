using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Timespinner.Core.Specifications;

[Serializable]
public sealed class TextureAtlasFrameSpecification
{
	private readonly List<Rectangle> _frameSources = new List<Rectangle>();

	public bool IsSelected { get; set; }

	public bool DoesNewRowUseStartX { get; set; }

	public int Count { get; set; }

	public int RowWidth { get; set; }

	public int StartIndex { get; set; }

	public Point FrameSize { get; set; }

	public Point StartCoordinates { get; set; }

	public List<Rectangle> FrameSources => _frameSources;

	public static TextureAtlasFrameSpecification FromAtlasFrame(TextureAtlasFrame frame)
	{
		TextureAtlasFrameSpecification textureAtlasFrameSpecification = new TextureAtlasFrameSpecification();
		textureAtlasFrameSpecification.DoesNewRowUseStartX = frame.DoesNewRowUseStartX;
		textureAtlasFrameSpecification.Count = frame.Count;
		textureAtlasFrameSpecification.RowWidth = frame.RowWidth;
		textureAtlasFrameSpecification.StartIndex = frame.StartIndex;
		textureAtlasFrameSpecification.FrameSize = frame.FrameSize;
		textureAtlasFrameSpecification.StartCoordinates = frame.StartCoordinates;
		return textureAtlasFrameSpecification;
	}

	public TextureAtlasFrame ToTextureAtlasFrame()
	{
		TextureAtlasFrame textureAtlasFrame = new TextureAtlasFrame();
		textureAtlasFrame.DoesNewRowUseStartX = DoesNewRowUseStartX;
		textureAtlasFrame.Count = Count;
		textureAtlasFrame.RowWidth = RowWidth;
		textureAtlasFrame.StartIndex = StartIndex;
		textureAtlasFrame.FrameSize = FrameSize;
		textureAtlasFrame.StartCoordinates = StartCoordinates;
		return textureAtlasFrame;
	}

	public void RefreshFrameSources(int textureWidth)
	{
		FrameSources.Clear();
		int num = (int)Math.Floor((float)textureWidth / (float)FrameSize.X);
		if (num <= 0)
		{
			num = 1;
		}
		Point point;
		if (StartIndex == 0)
		{
			point = StartCoordinates;
		}
		else
		{
			int num2 = StartIndex % num;
			int num3 = StartIndex / num;
			point = new Point(num2 * FrameSize.X, num3 * FrameSize.Y);
		}
		Point point2 = point;
		int num4 = StartIndex % num;
		for (int i = 0; i < Count; i++)
		{
			FrameSources.Add(new Rectangle(point2.X, point2.Y, FrameSize.X, FrameSize.Y));
			num4++;
			if (num4 < RowWidth)
			{
				point2.X += FrameSize.X;
				continue;
			}
			point2.X = (DoesNewRowUseStartX ? point.X : 0);
			point2.Y += FrameSize.Y;
			num4 = 0;
		}
	}

	public TextureAtlasFrameSpecification Duplicate()
	{
		TextureAtlasFrameSpecification textureAtlasFrameSpecification = new TextureAtlasFrameSpecification();
		textureAtlasFrameSpecification.DoesNewRowUseStartX = DoesNewRowUseStartX;
		textureAtlasFrameSpecification.Count = Count;
		textureAtlasFrameSpecification.RowWidth = RowWidth;
		textureAtlasFrameSpecification.StartIndex = StartIndex;
		textureAtlasFrameSpecification.FrameSize = FrameSize;
		textureAtlasFrameSpecification.StartCoordinates = StartCoordinates;
		return textureAtlasFrameSpecification;
	}
}
