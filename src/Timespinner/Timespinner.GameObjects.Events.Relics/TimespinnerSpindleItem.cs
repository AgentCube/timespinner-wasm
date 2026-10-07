using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Relics;

internal sealed class TimespinnerSpindleItem : RelicItemBase
{
	private readonly Action _onPickedUpAction;

	public TimespinnerSpindleItem(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, SpriteSheet sprite, Action onPickedUp)
		: base(inLevel, inPosition, inID, objectSpec, sprite)
	{
		_onPickedUpAction = onPickedUp;
		ChangeAnimation(26);
	}

	internal override void OnPickedUp()
	{
		_onPickedUpAction();
	}
}
