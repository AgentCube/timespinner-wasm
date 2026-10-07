using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class HangarTowerLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 28;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.4f, 0.5f, 1f, 1f);

	public HangarTowerLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = Position.Add(0, -36);
		_sprite = _level.GCM.SpLanterns;
		ChangeAnimation(170);
		Bbox = new Rectangle(0, 0, 12, 8);
		_doesUseAppendageCollision = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		_doAppendagesInheritDrawColor = false;
		base.DoesFlicker = true;
		base.GlowRadius = 28;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		base.IsGlowing = false;
		base.LanternGlowOffset = new Point(1, 0);
		SnapBboxToPosition();
		SnapFrameToBbox();
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternBrazierBreak, Position);
		CreateDebris(projectile);
		base.Explode(projectile);
	}
}
