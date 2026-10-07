using Microsoft.Xna.Framework;

namespace Timespinner.Core.Specifications;

public class MinimapMarker
{
	public enum EMinimapMarkerColor
	{
		Blue,
		Red,
		Grey,
		Purple,
		Green
	}

	public bool IsVisible { get; set; }

	public EMinimapRoomColor EraColor { get; set; }

	public EMinimapMarkerColor MarkerColor { get; set; }

	public Point Location { get; set; }

	public Vector2 DrawLocationPosition { get; set; }
}
