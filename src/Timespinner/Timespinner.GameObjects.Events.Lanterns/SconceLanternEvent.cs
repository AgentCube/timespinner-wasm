using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class SconceLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 48;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 1f);

	public SconceLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = Position.Add(-2, -5);
		_sprite = _level.GCM.SpLanterns;
		ChangeAnimation(2, 4, 0.1f, EAnimationType.Cycle);
		BboxOffset = new Point(-4, 4);
		Bbox = new Rectangle(0, 0, 16, 16);
		DrawOrigin = new Vector2(4f, 4f);
		Position = Position.Add(2, -3);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		base.DoesFlicker = true;
		base.GlowRadius = 48;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		SnapBboxToPosition();
		SnapFrameToBbox();
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternExtinguish, Position);
		_level.AddAnimation(EBattleAnimationType.ExtinguishSmoke, Position);
		base.Explode(projectile);
	}
}
