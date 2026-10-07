using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events;

internal sealed class TimespinnerGear : TimespinnerAppendage
{
	private static readonly Point GearAppendageOffset1 = new Point(63, -97);

	private static readonly Point GearAppendageOffset2 = new Point(69, -71);

	private static readonly Point GearAppendageOffset3 = new Point(43, -71);

	public TimespinnerGear(TheTimespinner parent, Level inLevel, SpriteSheet inSprite, int gearIndex)
		: base(parent, new Point(1, 1), new Point(14, 14), inLevel, inSprite, ETimespinnerAppendageType.Gear)
	{
		DrawOrigin = new Vector2(14f, 14f);
		switch (gearIndex)
		{
		case 0:
			base.AnchorOffset = GearAppendageOffset1;
			break;
		case 1:
			base.AnchorOffset = GearAppendageOffset2;
			_isReverseRotation = true;
			break;
		default:
			base.AnchorOffset = GearAppendageOffset3;
			_wheelRotation += 0.33f;
			break;
		}
	}
}
