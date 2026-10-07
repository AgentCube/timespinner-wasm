using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class PrologueLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 33;

	private const int FlameGlowCircleCount = 5;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.7f, 0.25f, 0.6f);

	public PrologueLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = Position.Add(0, -23);
		_sprite = _level.GCM.SpLanterns;
		IsFacingLeft = objectSpec == null || !objectSpec.IsFlippedHorizontally;
		ChangeAnimation(156);
		Bbox = new Rectangle(0, 0, 10, 8);
		_doesUseAppendageCollision = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		base.DoesDropLoot = false;
		_doAppendagesInheritDrawColor = false;
		_doAppendagesMatchImageFacing = true;
		base.DoesFlicker = true;
		base.GlowRadius = 33;
		base.GlowCircleCount = 5;
		base.OrbGlowColor = FlameGlowColor;
		base.IsGlowing = false;
		base.LanternGlowOffset = new Point(0, 1);
		SnapBboxToPosition();
		SnapFrameToBbox();
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternBrazierBreak, Position);
		CreateDebris(projectile);
		_isDead = true;
		base.Explode(projectile);
	}
}
