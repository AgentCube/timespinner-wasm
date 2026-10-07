using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class MetropolisLanternLibraryHangEvent : BaseLantern
{
	private const int FlameGlowRadius = 56;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.8f, 0.6f, 0.4f, 0.9f);

	public MetropolisLanternLibraryHangEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = new Point(Position.X + 8, Position.Y);
		_sprite = _level.GCM.SpLanterns;
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 32, 16);
		base.LanternGlowOffset = new Point(0, -8);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		base.DoesFlicker = false;
		base.GlowRadius = 56;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		SnapBboxToPosition();
		SnapFrameToBbox();
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 0)
		{
			SetCharacterSequence(base.CharacterSpecification.Sequences[0]);
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
