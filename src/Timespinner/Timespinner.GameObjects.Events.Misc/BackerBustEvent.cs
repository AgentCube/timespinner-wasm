using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class BackerBustEvent : GameEvent
{
	private const int BaseFrameIndex = 24;

	private readonly int _bustIndex;

	public BackerBustEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpBackerPortraits;
		_bustIndex = objectSpec.Argument - 4;
		ChangeAnimation(24 + _bustIndex);
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 32, 54);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doAppendagesMatchImageFacing = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		_isAffectedByTime = true;
		base.Appendages.Clear();
	}
}
