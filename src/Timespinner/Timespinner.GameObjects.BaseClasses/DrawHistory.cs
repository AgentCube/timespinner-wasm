using Microsoft.Xna.Framework;

namespace Timespinner.GameObjects.BaseClasses;

public class DrawHistory
{
	public bool IsImageFacingLeft { get; set; }

	public float Rotation { get; set; }

	public Point DrawPosition { get; set; }

	public Rectangle FrameSource { get; set; }

	public Color DrawColor { get; set; }
}
