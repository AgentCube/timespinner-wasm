using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class ForestLampLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 33;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 0.7f);

	public ForestLampLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpLanterns;
		Bbox = new Rectangle(0, 0, 16, 16);
		DrawOrigin = new Vector2(0f, 0f);
		Position = Position.Add((!objectSpec.IsFlippedHorizontally) ? 3 : (-3), -3);
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
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 0)
		{
			SetCharacterSequence(base.CharacterSpecification.Sequences[0]);
		}
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternGlassBreak, Position);
		_level.AddAnimation(EBattleAnimationType.ExtinguishSmoke, Position);
		_level.AddAnimation(EBattleAnimationType.GlassShatter, Bbox.Center, ETeamSide.Neutral, projectile.VisibleVelocity.X < 0f);
		CreateDebris(projectile);
		base.Explode(projectile);
	}
}
