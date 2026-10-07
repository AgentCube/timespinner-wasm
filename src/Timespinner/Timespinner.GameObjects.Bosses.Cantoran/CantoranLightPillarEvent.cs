using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Cantoran;

internal class CantoranLightPillarEvent : GameEvent
{
	private const int LightPillarAnimationIndex = 71;

	private const float TimeToFadeIn = 0.25f;

	private const float TimeToLinger = 2f;

	private const float TimeToFadeOut = 0.1f;

	private const float TimeBeforeFadingOut = 2.25f;

	private const float TotalLifetime = 2.35f;

	private static readonly Color BaseDrawColor = new Color(0.175f, 0.15f, 0.1f, 0.075f);

	private bool _isFinished;

	private bool _hasBeenAdded;

	private float _lifeTimer;

	public CantoranLightPillarEvent(Level inLevel, Point inPosition, ObjectTileSpecification objectSpec, SpriteSheet sprite)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_sprite = sprite;
		ChangeAnimation(71);
		Bbox = new Rectangle(0, 0, 24, 64);
		base.CanBeTriggered = false;
		_isSolid = false;
		base.IsAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_isFinished)
		{
			_lifeTimer += delta;
			if (_lifeTimer < 2.35f)
			{
				float num = ((_lifeTimer < 0.25f) ? (_lifeTimer / 0.25f) : ((!(_lifeTimer < 2.25f)) ? (1f - (_lifeTimer - 2.25f) / 0.1f) : 1f));
				base.DrawColor = BaseDrawColor * num;
			}
			else
			{
				_isFinished = true;
				SilentKill();
			}
		}
		base.Update(delta);
	}

	internal bool Reset(Point position)
	{
		bool flag = _isFinished || !_hasBeenAdded;
		if (flag)
		{
			base.ID = -1;
		}
		_hasBeenAdded = true;
		_isFinished = false;
		Position = position;
		SnapBboxToPosition();
		_lifeTimer = 0f;
		base.DrawColor = Color.Transparent;
		return flag;
	}
}
