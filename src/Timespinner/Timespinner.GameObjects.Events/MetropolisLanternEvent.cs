using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Lanterns;

namespace Timespinner.GameObjects.Events;

internal sealed class MetropolisLanternEvent : BaseLantern
{
	private const float OscillFrequencyY = 2f;

	private const float OscillRadius = 3f;

	private static readonly Vector4 BaseOrbGlowColor = new Vector4(0.3f, 0.5f, 0.9f, 0.8f);

	private Point _basePosition;

	internal MetropolisLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpLanterns;
		_isAffectedByTime = true;
		_basePosition = new Point(Position.X, Position.Y - 6);
		Position = _basePosition;
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 24, 24);
		IsFacingLeft = true;
		_isAffectedByGravity = false;
		_doesPersist = true;
		_isRepeatedTrigger = true;
		ChangeAnimation(-1);
		base.OrbGlowColor = BaseOrbGlowColor;
		base.DoesRegenerate = false;
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		if (!base.IsFrozen)
		{
			float num = (float)Math.Sin((0f - _oscillDelta) * 2f) * 3f;
			num -= 5f;
			Position = new Point(_basePosition.X, (int)((float)_basePosition.Y + num));
		}
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternGlassBreak, Position);
		_level.AddAnimation(EBattleAnimationType.GlassShatter, Bbox.Center, ETeamSide.Neutral, projectile.VisibleVelocity.X < 0f);
		CreateDebris(projectile);
		base.Explode(projectile);
	}
}
