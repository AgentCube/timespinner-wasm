using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class MetropolisLanternLibraryStandEvent : BaseLantern
{
	private const int FlameGlowRadius = 31;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 1f);

	public MetropolisLanternLibraryStandEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = Position.Add(-1, -7);
		_sprite = _level.GCM.SpLanterns;
		ChangeAnimation(9, 4, 0.1f, EAnimationType.Cycle);
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 3, 9);
		base.LanternGlowOffset = new Point(1, 2);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		base.DoesFlicker = true;
		base.GlowRadius = 31;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		SnapBboxToPosition();
		SnapFrameToBbox();
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternGlassBreak, Position);
		_level.AddAnimation(EBattleAnimationType.ExtinguishSmoke, Position);
		_level.AddAnimation(EBattleAnimationType.GlassShatter, Bbox.Center, ETeamSide.Neutral, projectile.VisibleVelocity.X < 0f);
		base.Explode(projectile);
	}
}
