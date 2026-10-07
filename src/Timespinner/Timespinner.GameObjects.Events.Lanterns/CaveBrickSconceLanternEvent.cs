using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class CaveBrickSconceLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 48;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 1f);

	private readonly Appendage _wallLightAppendage;

	public CaveBrickSconceLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpLanterns;
		ChangeAnimation(14, 4, 0.1f, EAnimationType.Cycle);
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 16, 16);
		DrawOrigin = new Vector2(8f, 8f);
		Position = Position.Add(0, -4);
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
		_wallLightAppendage = new Appendage(this, new Point(16, 32), Point.Zero, _level, _sprite)
		{
			DrawPriority = -1
		};
		_wallLightAppendage.ChangeAnimation(18);
		_wallLightAppendage.Position = inPosition;
		_appendages.Add(_wallLightAppendage);
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternExtinguish, Position);
		_level.AddAnimation(EBattleAnimationType.ExtinguishSmoke, Position);
		base.Explode(projectile);
	}
}
