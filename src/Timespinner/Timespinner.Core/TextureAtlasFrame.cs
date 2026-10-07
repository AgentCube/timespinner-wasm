using Microsoft.Xna.Framework;

namespace Timespinner.Core;

public class TextureAtlasFrame
{
	public bool DoesNewRowUseStartX { get; set; }

	public int Count { get; set; }

	public int RowWidth { get; set; }

	public int StartIndex { get; set; }

	public Point FrameSize { get; set; }

	public Point StartCoordinates { get; set; }
}
