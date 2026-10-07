using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class LakeCoralLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 41;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.6f, 0.8f, 0.3f, 1f);

	public LakeCoralLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpLanterns;
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 16, 16);
		base.ItemDropOffset = new Point(0, -20);
		_doesUseAppendageCollision = true;
		_doesDrawBaseSprite = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DebrisType = DebrisEvent.EDebrisDeathType.Dust;
		base.DoesRegenerate = false;
		_doAppendagesInheritDrawColor = false;
		base.DoesFlicker = true;
		base.GlowRadius = 41;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		base.IsGlowing = false;
		base.LanternGlowOffset = new Point(1, -20);
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
