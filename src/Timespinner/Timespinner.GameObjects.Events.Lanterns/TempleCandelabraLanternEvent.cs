using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class TempleCandelabraLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 48;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 1f);

	public TempleCandelabraLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = Position.Add(2, -26);
		_sprite = _level.GCM.SpLanterns;
		ChangeAnimation(14, 4, 0.1f, EAnimationType.Cycle);
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 16, 16);
		DrawOrigin = new Vector2(8f, 8f);
		_doesUseAppendageCollision = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		_doAppendagesInheritDrawColor = false;
		base.DoesFlicker = true;
		base.GlowRadius = 48;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		base.IsGlowing = false;
		base.LanternGlowOffset = new Point(0, 4);
		SnapBboxToPosition();
		SnapFrameToBbox();
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternBrazierBreak, Position);
		_level.AddAnimation(EBattleAnimationType.ExtinguishSmoke, Position);
		CreateDebris(projectile);
		base.Explode(projectile);
	}
}
