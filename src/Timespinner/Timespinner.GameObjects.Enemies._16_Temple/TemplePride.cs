using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._16_Temple;

internal class TemplePride : Monster
{
	public TemplePride(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		Bbox = new Rectangle(0, 0, 16, 16);
		ChangeAnimation(-1);
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_isAffectedByGravity = false;
		_isFlying = true;
	}
}
