using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class LakeCandleLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 16;

	private const int FlameGlowCircleCount = 4;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 1f);

	public LakeCandleLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = Position.Add(0, -21);
		_sprite = _level.GCM.SpLanterns;
		ChangeAnimation(9, 4, 0.1f, EAnimationType.Cycle);
		Bbox = new Rectangle(0, 0, 3, 9);
		_doesUseAppendageCollision = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		_doAppendagesInheritDrawColor = false;
		base.DoesFlicker = true;
		base.GlowRadius = 16;
		base.GlowCircleCount = 4;
		base.OrbGlowColor = FlameGlowColor;
		base.IsGlowing = false;
		base.LanternGlowOffset = new Point(1, 0);
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
