using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class CaveMineLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 33;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 0.7f);

	private readonly Appendage _lanternLightAppendage;

	public CaveMineLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpLanterns;
		ChangeAnimation(9, 4, 0.1f, EAnimationType.Cycle);
		BboxOffset = new Point(-5, -8);
		Bbox = new Rectangle(0, 0, 16, 16);
		DrawOrigin = new Vector2(0f, 0f);
		Position = Position.Add(1, -4);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		base.DoesFlicker = true;
		base.GlowRadius = 33;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		base.LanternGlowOffset = new Point(0, 5);
		SnapBboxToPosition();
		SnapFrameToBbox();
		_lanternLightAppendage = new Appendage(this, new Point(10, 14), Point.Zero, _level, _sprite)
		{
			DrawPriority = -1
		};
		_lanternLightAppendage.ChangeAnimation(20);
		_lanternLightAppendage.Position = inPosition.Add(0, 0);
		_appendages.Add(_lanternLightAppendage);
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternGlassBreak, Position);
		_level.AddAnimation(EBattleAnimationType.ExtinguishSmoke, Position);
		_level.AddAnimation(EBattleAnimationType.GlassShatter, Bbox.Center, ETeamSide.Neutral, projectile.VisibleVelocity.X < 0f);
		base.Explode(projectile);
	}
}
