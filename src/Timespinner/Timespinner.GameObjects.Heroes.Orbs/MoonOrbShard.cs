using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class MoonOrbShard : Appendage
{
	private const float TimeBetweenFollowFrames = 0.0166666f;

	private readonly Mobile _largerSibling;

	private Point _secondToLastSiblingPosition;

	private Point _thirdToLastSiblingPosition;

	private float _followFrameTimer;

	public MoonOrbShard(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite, Mobile largerSibling)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		base.DrawPriority = -1;
		_largerSibling = largerSibling;
	}

	internal void InitializeShard(Point position)
	{
		Position = position;
		_secondToLastSiblingPosition = position;
		_thirdToLastSiblingPosition = position;
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		_followFrameTimer -= delta;
		if (_followFrameTimer <= 0f)
		{
			_followFrameTimer = 0.0166666f;
			Position = _thirdToLastSiblingPosition;
			_thirdToLastSiblingPosition = _secondToLastSiblingPosition;
			_secondToLastSiblingPosition = _largerSibling.LastPosition;
			SnapBboxToPosition();
			SnapFrameToBbox();
			RefreshDrawPos();
		}
	}

	internal void ChangeRoom(Point position)
	{
		Position = position;
		_secondToLastSiblingPosition = position;
		_thirdToLastSiblingPosition = position;
	}
}
