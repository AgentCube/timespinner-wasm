using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class HangarHangingLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 40;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.4f, 0.9f);

	public HangarHangingLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = new Point(Position.X, Position.Y + 4);
		_sprite = _level.GCM.SpLanterns;
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 16, 16);
		base.LanternGlowOffset = new Point(0, 4);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		IsFacingLeft = true;
		base.DoesRegenerate = false;
		base.DoesFlicker = true;
		base.GlowRadius = 40;
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
