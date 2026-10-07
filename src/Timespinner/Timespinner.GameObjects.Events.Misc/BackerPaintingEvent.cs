using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class BackerPaintingEvent : GameEvent
{
	private const int PortraitShadowIndex = 38;

	private const int PortraitCount = 21;

	private static readonly Color ShadowColor = new Color(96, 96, 96, 160);

	private bool _wasVisible;

	public BackerPaintingEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpBackerPortraits;
		ChangeAnimation(GetNewPortraitIndex(_level));
		Position = new Point(Position.X, Position.Y - 9);
		Bbox = new Rectangle(Position.X, Position.Y, 32, 48);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = false;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		_isAffectedByTime = true;
		base.Appendages.Clear();
		Appendage appendage = new Appendage(this, new Point(32, 48), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			DrawPriority = 1,
			DrawColor = ShadowColor
		};
		appendage.ChangeAnimation(38);
		_appendages.Add(appendage);
	}

	public override void Update(float delta)
	{
		bool flag = _level.VisibleArea.Intersects(Bbox);
		if (!flag && _wasVisible)
		{
			ChangeAnimation(GetNewPortraitIndex(_level));
		}
		_wasVisible = flag;
		base.Update(delta);
	}

	private static int GetNewPortraitIndex(Level level)
	{
		return level.NextRandomInt(0, 20);
	}
}
